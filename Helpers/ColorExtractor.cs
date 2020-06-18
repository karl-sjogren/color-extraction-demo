using System;
using System.IO;
using System.Linq;
using System.Text;
using color_extraction_demo.Models;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace color_extraction_demo.Helpers {
    public static class ColorExtractor {
        public static ImageColors GetCached(Stream imageStream, string filename) {
            var cachePath = "wwwroot\\cache\\" + filename + ".json";
            if(File.Exists(cachePath)) {
                var json = File.ReadAllText(cachePath, Encoding.UTF8);
                return JsonConvert.DeserializeObject<ImageColors>(json);
            }

            var result = ProcessImage(imageStream, filename);

            var resultJson = JsonConvert.SerializeObject(result, Formatting.Indented);
            File.WriteAllText(cachePath, resultJson, Encoding.UTF8);

            return result;
        }

        public static ImageColors ProcessImage(Stream imageStream, string filename) {
            var image = Image.Load<Rgba32>(imageStream);

            try {
                if(image.Width > 800 || image.Height > 800)
                    image.Mutate(i => i.Resize(800, 800));

                var imageFrame = image.Frames[0];
                var quantizer = new WuQuantizer(new QuantizerOptions { MaxColors = 12 });
                var frameQuantizer = quantizer.CreatePixelSpecificQuantizer<Rgba32>(Configuration.Default);

                frameQuantizer.BuildPaletteAndQuantizeFrame(imageFrame, image.Bounds());
                var quantizedColors = frameQuantizer.QuantizeFrame(imageFrame, image.Bounds());

                var paletteColors = quantizedColors.Palette;

                var frame = image.Frames[0];
                var totalPixels = frame.Width * frame.Height;
                frame.TryGetSinglePixelSpan(out var pixels);

                var counter = new double[paletteColors.Length];

                foreach(var pixel in pixels) {
                    var closestDistance = double.MaxValue;
                    var nearestColorIndex = -1;

                    for(var index = 0; index < paletteColors.Length; index++) {
                        var paletteColor = paletteColors.Span[index];
                        var distance = EuclideanDistance(paletteColor, pixel);

                        if(distance < closestDistance) {
                            closestDistance = distance;
                            nearestColorIndex = index;
                        }
                    }

                    counter[nearestColorIndex]++;
                }

                var colors = paletteColors.Span.ToArray().Select((color, idx) => {
                    var (hue, saturation, value) = ColorToHsv(color);

                    return new ExtractedColor {
                        Hsv = new HsvColor(hue, saturation, value),
                        Hex = color.ToHex().Substring(0, 6),
                        Fraction = counter[idx] / totalPixels
                    };
                });

                return new ImageColors {
                    Filename = filename,
                    Colors = colors.OrderByDescending(c => c.Fraction).ToList()
                };
            } finally {
                image?.Dispose();
            }
        }

        public static (float hue, float saturation, float value) ColorToHsv(Rgba32 color) {
            var min = (float)Math.Min(Math.Min(color.R, color.G), color.B);
            var value = (float)Math.Max(Math.Max(color.R, color.G), color.B);
            var delta = value - min;

            float saturation;
            if(Math.Abs(value) < double.Epsilon)
                saturation = 0;
            else
                saturation = delta / value;

            var hue = 0f;
            if(Math.Abs(saturation) < double.Epsilon) {
                hue = 0.0f;
            } else {
                if(Math.Abs(color.R - value) < double.Epsilon)
                    hue = (color.G - color.B) / delta;
                else if(Math.Abs(color.G - value) < double.Epsilon)
                    hue = 2f + (color.B - color.R) / delta;
                else if(Math.Abs(color.B - value) < double.Epsilon)
                    hue = 4f + (color.R - color.G) / delta;

                hue *= 60f;

                if(hue < 0.0f)
                    hue = hue + 360f;
            }

            return (hue, saturation, value / 255);
        }

        private static double EuclideanDistance(Rgba32 color1, Rgba32 color2) {
            var distance = Math.Pow(color1.R - color2.R, 2) + Math.Pow(color1.G - color2.G, 2) + Math.Pow(color1.B - color2.B, 2);
            return Math.Sqrt(distance);
        }
    }
}