using System.IO;
using System.Linq;
using color_extraction_demo.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.PixelFormats;

namespace color_extraction_demo.Controllers {
    public class ColorController : Controller {
        private readonly ILogger<ColorController> _logger;

        public ColorController(ILogger<ColorController> logger) {
            _logger = logger;
        }

        public IActionResult Index() {
            return View();
        }

        public IActionResult All([FromQuery]string hexColor = null) {
            var files = Directory.EnumerateFiles("wwwroot\\images");

            var result = files.Select(file => {
                using var stream = System.IO.File.OpenRead(file);

                var colors = ColorExtractor.GetCached(stream, Path.GetFileName(file));

                return colors;
            });

            if(!string.IsNullOrWhiteSpace(hexColor)) {
                var color = Rgba32.ParseHex(hexColor);
                var (hue, saturation, value) = ColorExtractor.ColorToHsv(color);

                const double hueRange = 255 * .15d;
                const double saturationRange = .15d;
                const double valueRange = .15d;

                result = result.Where(image => {
                    foreach(var imageColor in image.Colors) {
                        var hsv = imageColor.Hsv;
                        imageColor.MatchedColor = hsv.H - hueRange < hue && hsv.H + hueRange > hue &&
                            hsv.S - saturationRange < saturation && hsv.S + saturationRange > saturation &&
                            hsv.V - valueRange < value && hsv.V + valueRange > value;
                    }

                    return image.Colors.Any(x => x.MatchedColor);
                });
            }

            return Ok(result);
        }

        public IActionResult Privacy() {
            return View();
        }

    }
}
