using SkiaSharp;
using System.Drawing;

namespace PodNet.Boxify.Svg;

internal sealed class SKBitmapSource(SKBitmap bitmap) : IBoxBitmapSource, IDisposable
{
    public int Width => Bitmap.Width;

    public int Height => Bitmap.Height;

    public SKBitmap Bitmap { get; } = bitmap;

    public void Dispose() => Bitmap.Dispose();

    public Color GetPixel(int x, int y)
    {
        var pixel = Bitmap.GetPixel(x, y);
        return Color.FromArgb(pixel.Alpha, pixel.Red, pixel.Green, pixel.Blue);
    }
}