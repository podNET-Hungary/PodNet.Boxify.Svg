# PodNet.Boxify.Svg [![NuGet](https://img.shields.io/nuget/v/PodNet.Boxify.Svg)](https://www.nuget.org/packages/PodNet.Boxify.Svg/)
An extension for [`PodNet.Boxify`](https://github.com/podNET-Hungary/PodNet.Boxify/) that converts an SVG image to a rasterized format to be used for drawing box-drawings of images to text, files or terminals

## Usage

This package contains a single public type: `PodNet.Boxify.Svg.SvgConverter`. This type is used to convert an SVG image to a bitmap, which will in turn be used to draw Unicode box-drawings of the originally vector-based image (now rasterized). However, it also takes a transient dependency on the core `PodNet.Boxify` package.

```csharp
using PodNet.Boxify;
using PodNet.Boxify.Svg;

// Get your graphics from wherever
var podnet = """
<svg width="1024" height="1024" viewBox="0 0 1024 1024" xmlns="http://www.w3.org/2000/svg">
    <path stroke-width="4" stroke="#fff" d="M133 0v570m0 143v311m19-518v518M326 0v333m0 246v445m67-597v508M548 0v471m0 146v407M593 0v133m0 530v361M723 0v363m0 249v412m56-692v508M950 0v540m0 145v339m15-548v548M0 151h50M0 496h15m271-252h198M224 600h346m93-276h361m-24-174h24m-3 345h3"/>
    <path fill="#000" stroke-width="1" stroke="#fff" d="M192 192h640v640H192z"/>
    <text font-family="Open Sans" x="192" y="512" font-size="192px" text-anchor="middle" fill="#fff">
      <tspan x="512" y="472">pod</tspan><tspan x="512" y="672" font-weight="bold">NET</tspan>
    </text>
</svg>
""";

// You can set the Console's output encoding to properly display the characters. Usually only needed for high-res pixel palettes.
Console.OutputEncoding = System.Text.Encoding.UTF8;

// Choose from a range of palettes to use
var palette = PixelPalette.Halves;

// You can render only bitmaps. Use PodNet.Boxify.Bmp to wrap a simple System.Drawing.Bitmap, PodNet.Boxify.Svg to convert an
// SVG to a wrapped System.Drawing.Bitmap, or supply your own by implementing this simple interface.
IBoxBitmapSource bitmap = SvgConverter.Default.Convert(podnet, palette, maxRows: 40);

var boxDrawing = Renderer.Default.Render(bitmap, palette);
Console.WriteLine(boxDrawing);
```

Which outputs on the console:

```cmd
         ██             ██                ██ ██        ██                ██     
         ██             ██                ██ ██        ██                ██     
         ██             ██                ██ ██        ██                ██     
         ██             ██                ██ ██        ██                ██     
         ██             ██                ██ ██        ██                ██     
▄▄▄▄     ██             ██                ██ ▀▀        ██                ██   ▄▄
▀▀▀▀     ██             ██                ██           ██                ██   ▀▀
         ██   ▄▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▄       ██     
         ██   █                                                  █       ██     
         ██   █                                                  █       ██     
         ██   █                                                  █       ██     
         ██   █                                                  █       ██     
         ██   █                                                  ███████████████
         ██   █                                   ██             █       ██     
         ██   █             ▄▄▄▄▄▄    ▄▄▄▄    ▄▄▄▄██             █       ██     
         ██   █             ██▀  ██ ██▀  ▀█▄ ██▀  ██             █       ██     
         ██   █             ██   ██ ██    ██ ██   ██             █       ██     
         ██   █             ██  ▄██ ██▄  ▄█▀ ██▄  ██             █       ██     
         ██   █             ██▀▀▀▀   ▀▀▀▀▀    ▀▀▀▀▀▀             █       ██▄    
██       ██▄▄ █             ██                                   █       ███   █
         ████ █                                                  █       ███    
         ████ █            ███▄   ███ ████████████████           █       ▀██    
         ▀▀██ █            █████  ███ ███       ███              █        ██    
           ██ █            ██████ ███ ███████   ███              █        ██    
           ██ █            ███ ▀█████ ███       ███              █        ██    
           ██ █            ███  ▀████ ███▄▄▄▄   ███              █        ██    
           ██ █            ▀▀▀   ▀▀▀▀ ▀▀▀▀▀▀▀   ▀▀▀              █       ▄██    
         ▄▄██ █                                                  █       ███    
         ████ █                                                  █       ███    
         ████ █                                                  █       ███    
         ████ █                                                  █       ███    
         ████ █                                                  █       ███    
         ████ ▀▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▀       ███    
         ████           ██    ██          ██ ██        ██                ███    
         ████           ██    ██          ██ ██        ██                ███    
         ████           ██    ██          ██ ██        ██                ███    
         ████           ██    █▀          ██ ██        ██                ███    
         ████           ██                ██ ██        ██                ███    
         ████           ██                ██ ██        ██                ███    
         ████           ██                ██ ██        ██                ███    
```

There are two overloads for the conversion available:
```csharp
IBoxBitmapSource Convert(string svgContent, int? maxWidth = null, int? maxHeight = null, float scaleX = 1f, float scaleY = 1f)
IBoxBitmapSource Convert(string svgContent, PixelPalette palette, int? maxRows = null, int? maxColumns = null, float scaleX = 2f, float scaleY = 1f)
```

Note that this type does:
- resizing given a `PodNet.Boxify.PixelPalette` to try and keep the aspect ratio of the rendered image, given the default approximately 1:2 aspect ratio of characters in many UI endpoints (clients), and given that the pixel palettes often define characters that are themselves a set of pixels (sextants, octants etc.),
- SVG fixup; by default, this method does nothing, but you can override `SvgConverter.FixupSvg(SvgDocument svgDocument)` in your inherited class to do any fixup you wish before the SVG content is rasterized.

Resizing works by scaling the image along the X and Y axis by `scaleX` and `scaleY`, then resizing as needed while keeping the aspect ratio and taking into account the `maxWidth` and `maxHeight` to fit. This essentially produces a stretched/squashed image that will be normalized during the rendering process.

The second overload (which calls into the first one) takes a `PixelPalette` to remove the "calculation" needed on the part of the user. If you're rendering the box-drawing elsewhere, you can configure the `scaleX` and `scaleY` parameters according to your needs, for example, to draw square-shaped characters when using some specific fonts, or when using terminals/IDEs/viewers with character outputs set to other than 1:2 aspect ratio characters.

Also consider the options provided by `PodNet.Boxify.PixelAnalyzer` (which take into account the alpha channel, HSB brightness, or both), or supply your own implementation by inheriting from it to determine which pixels should be considered "set" (monochrome) or how bright (grayscale images).

You can also render colorized images (including full RGB gradients), as `PodNet.Boxify` already support that.

For more details on how to use [`PodNet.Boxify`](https://github.com/podNET-Hungary/PodNet.Boxify/), including colorization, see the repo at https://github.com/podNET-Hungary/PodNet.Boxify/.

This package depends on the [Svg.Skia](https://github.com/wieslawsoltes/Svg.Skia) library, which is used to rasterize the image, which in turn depends on [SkiaSharp](https://github.com/mono/SkiaSharp) (v1.0.0 of the package was using the [SVG](https://github.com/vvvv/SVG) library, which in turn used `System.Drawing.Common`, which caused incompatibility problems across newer runtimes and when used on non-Windows platforms).

Do note that rendering SVGs outside of their "native habitats" (browsers) can lead to some unexpected results, especially when using advanced features like new CSS features or masking in SVG.

## Contributing and Support

This project is intended to be widely usable, but no warranties are provided. If you want to contact us, feel free to do so in the org's [[Discussions](https://github.com/orgs/podNET-Hungary/discussions/)], at our website at [podnet.hu](https://podnet.hu), or find us anywhere from [LinkedIn](https://www.linkedin.com/company/podnet-hungary/) to [Meetup](https://www.meetup.com/budapest-net-meetup/), [YouTube](https://www.youtube.com/@podNET) or [X](https://twitter.com/podNET_Hungary).

Any kinds of contributions from issues to PRs and open discussions are welcome!

Don't forget to give us a ⭐ if you like this repo (it's free to give kudos!) or share it on socials!

## Sponsorship

If you're using our work or like what you see, consider supporting us. Every bit counts. 🙏 [See here for more info.](https://github.com/podNET-Hungary/PodNet.NuGet.Core/blob/main/src/PodNet.NuGet.Core/build/SPONSORS.md)