using PodNet.Boxify.Bmp;
using Svg;
using System.Drawing;

namespace PodNet.Boxify.Svg;

/// <summary>
/// Handles converting an SVG image to a box-art-compatible source for <see cref="Renderer"/>.
/// You can inherit from this class to override different aspects of the conversion process.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Note that this is not a framework component for Boxify but a convenience class to be invoked directly by user code.</item>
/// <item>Also do note that converting a bitmap to the corresponding box-art doesn't do resizing/aspect ratio conversion/pixel
/// ratio conversion on its own. You can use the <a href="https://www.nuget.org/packages/Svg">Svg</a> package to do image
/// manipulation on SVGs (besides this class), or some functions from <a href="https://www.nuget.org/packages/System.Drawing.Common">
/// System.Drawing.Common</a> as they are transiently referenced through the current package.</item>
/// </list>
/// </remarks>
public class SvgConverter
{
    /// <summary>The protected constructor for the converter. The default implementation is stateless,
    /// so users should prefer to use the <see cref="Default"/> instance.</summary>
    protected SvgConverter() { }

    /// <summary>The default converter instance.</summary>
    public static SvgConverter Default { get; } = new();

    /// <summary>Create a <see cref="IBoxBitmapSource"/> wrapper around a rasterized image from the provided SVG.</summary>
    /// <remarks>The default implementation calls, in order:
    /// <list type="bullet">
    /// <item><see cref="CreateSvg(string)"/></item>
    /// <item><see cref="FixupSvg(SvgDocument)"/></item>
    /// <item><see cref="RasterizeSvg(SvgDocument, int?, int?, float, float)"/></item>
    /// </list>
    /// </remarks>
    /// <param name="svgContent">The full content of the SVG (XML).</param>
    /// <param name="maxWidth">The maximum width the bitmap should be rendered in (while keeping the aspect ratio).</param>
    /// <param name="maxHeight">The maximum width the bitmap should be rendered in (while keeping the aspect ratio).</param>
    /// <param name="scaleX">Indicates the horizontal scaling to use when rasterizing the image. Useful for when characters
    /// represent multiple pixels (or fractions) in width and when rendered characters are not square-shaped.</param>
    /// <param name="scaleY">Indicates the vertical scaling to use when rasterizing the image. Useful for when characters
    /// represent multiple pixels (or fractions) in height and when rendered characters are not square-shaped.</param>
    public virtual IBoxBitmapSource Convert(string svgContent, int? maxWidth = null, int? maxHeight = null, float scaleX = 1f, float scaleY = 1f)
    {
        var svgDocument = CreateSvg(svgContent);
        FixupSvg(svgDocument);
        return RasterizeSvg(svgDocument, maxWidth, maxHeight, scaleX, scaleY);
    }

    /// <summary>Create a <see cref="IBoxBitmapSource"/> wrapper around a rasterized image from the provided SVG.</summary>
    /// <remarks>This is a convenience method for applying scaling transforms using the provided <paramref name="palette"/>.
    /// The dimensions and scaling factors are derived appropriately from the provided palette and parameters. For more info,
    /// see the <see cref="Convert(string, int?, int?, float, float)"/> overload.<br/>
    /// Default scaling is 2f for <paramref name="scaleX"/> and 1f for <paramref name="scaleY"/>, because many monospace
    /// renderers (like IDEs) use close to approximately 1x2 sized rectangles for the monospaced characters.
    /// </remarks>
    /// <param name="svgContent">The full content of the SVG (XML).</param>
    /// <param name="maxRows">The maximum number of rows in the output box-art.</param>
    /// <param name="maxColumns">The maximum number of columns (characters, can be multiple for each code point) in the
    /// output box-art.</param>
    /// <param name="scaleX">Additional scaling to use on the X axis, besides the one provided by <see cref="PixelPalette.PixelWidth"/>.</param>
    /// <param name="scaleY">Additional scaling to use on the Y axis, besides the one provided by <see cref="PixelPalette.PixelHeight"/>.</param>
    /// <param name="palette">The palette to derive the scaling information from.</param>
    public virtual IBoxBitmapSource Convert(string svgContent, PixelPalette palette, int? maxRows = null, int? maxColumns = null, float scaleX = 2f, float scaleY = 1f)
        => Convert(svgContent, maxColumns * palette.PixelWidth, maxRows * palette.PixelHeight, palette.PixelWidth * scaleX, palette.PixelHeight * scaleY);

    /// <summary>Creates an <see cref="SvgDocument"/> from the provided string content.</summary>
    /// <remarks>The default implementation calls <see cref="SvgDocument.FromSvg{T}(string)"/>.</remarks>
    /// <param name="svgContent">The full string (including XML header) of the SVG.</param>
    /// <returns>The parsed SVG document.</returns>
    protected virtual SvgDocument CreateSvg(string svgContent)
        => SvgDocument.FromSvg<SvgDocument>(svgContent);

    /// <summary>A shorthand for the default <see cref="SvgColourServer"/> using <see cref="Color.White"/>.
    /// Used by the default <see cref="FixupSvg(SvgDocument)"/>.</summary>
    protected static SvgColourServer White { get; } = new(Color.White);

    /// <summary>Prepares the SVG to be rasterized by making changes to the SVG structure. This is essential because most SVG
    /// defaults are optimal for web-based rendering, and not box-art rendering as required by Boxify.</summary>
    /// <remarks>The default implementation sets all <see cref="SvgElement"/> descendants of the <paramref name="svgDocument"/>
    /// to have <see cref="White"/> <see cref="SvgElement.Fill"/>, <see cref="SvgElement.Color"/> and <see cref="SvgElement.Stroke"/> 
    /// if not otherwise explicitly set to <see cref="SvgPaintServer.None"/>. <a href="https://svg-net.github.io/SVG/articles/Faq.html#how-to-render-an-svg-image-to-a-single-color-bitmap-image">See source for details.</a></remarks>
    /// <param name="svgDocument">The document to fix up.</param>
    protected virtual void FixupSvg(SvgDocument svgDocument)
    {
        foreach (var element in svgDocument.Descendants())
        {
            if (element.Fill == SvgPaintServer.NotSet)
                element.Fill = White;
            if (element.Stroke == SvgPaintServer.NotSet)
                element.Stroke = White;
        }
    }

    /// <summary>
    /// Calculates the target dimensions from <paramref name="svgDocument"/> for the rasterized image given either, neither
    /// or both of the max width and height, while keeping the image's intrinsic aspect ratio, but accounting for the possibility
    /// of scaling on the X and Y axes for non-square shaped pixels or custom render targets.
    /// </summary>
    /// <param name="svgDocument">The document to calculate the target values from.</param>
    /// <param name="maxWidth">The expected maximum width of the rasterized image. Can be null if ignored.</param>
    /// <param name="maxHeight">The expected maximum height of the rasterized image. Can be null if ignored.</param>
    /// <param name="scaleX">Scales the document on the X axis. The original aspect ratio is skewed on the X axis if X != 1f.</param>
    /// <param name="scaleY">Scales the document on the Y axis. The original aspect ratio is skewed on the Y axis if Y != 1f.</param>
    /// <returns>The target width and height of the image to be rasterized. Either, neither or both can be ZERO if ignored.</returns>
    protected virtual (int TargetWidth, int TargetHeight) GetTargetDimensions(SvgDocument svgDocument, int? maxWidth, int? maxHeight, float scaleX, float scaleY)
    {
        if (scaleX <= 0f)
            throw new ArgumentOutOfRangeException(nameof(scaleX), scaleX, "Value should be positive.");
        if (scaleY <= 0f)
            throw new ArgumentOutOfRangeException(nameof(scaleY), scaleY, "Value should be positive.");
        var (scaledWidth, scaledHeight) = (svgDocument.Width * scaleX, svgDocument.Height * scaleY);
        var aspectRatio = scaledWidth / scaledHeight;
        var (w, h) = (maxWidth ?? (int)scaledWidth, maxHeight ?? (int)scaledHeight);
        return (w / aspectRatio <= h) ? (w, (int)(w / aspectRatio)) : ((int)(h * aspectRatio), h);
    }

    /// <summary>Rasterizes the given <paramref name="svgDocument"/> to a <see cref="BitmapSource"/>.</summary>
    /// <remarks>The default implementation calls <see cref="GetTargetDimensions(SvgDocument, int?, int?, float, float) "/>
    /// and <see cref="SvgDocument.Draw(int, int)"/>.</remarks>
    /// <param name="svgDocument">The image to rasterize.</param>
    /// <param name="maxWidth">The max width to fit the resulting image into. Can be null if ignored.</param>
    /// <param name="maxHeight">The max height to fit the resulting image into. Can be null if ignored.</param>
    /// <param name="scaleX">Scales the document on the X axis. The original aspect ratio is skewed on the X axis if X != 1f.</param>
    /// <param name="scaleY">Scales the document on the Y axis. The original aspect ratio is skewed on the Y axis if Y != 1f.</param>
    /// <returns>The rasterized image, wrapped in a <see cref="BitmapSource"/>.</returns>
    protected virtual IBoxBitmapSource RasterizeSvg(SvgDocument svgDocument, int? maxWidth, int? maxHeight, float scaleX, float scaleY)
    {
        var (width, height) = GetTargetDimensions(svgDocument, maxWidth, maxHeight, scaleX, scaleY);
        return new BitmapSource(svgDocument.Draw(width, height));
    }
}
