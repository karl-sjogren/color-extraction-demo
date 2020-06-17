using SixLabors.ImageSharp.ColorSpaces;

namespace color_extraction_demo.Models {
    public class ExtractedColor {
        public HsvColor Hsv { get; set; }
        public string Hex { get; set; }
        public double Fraction { get; set; }
        public bool MatchedColor { get; set; }
    }

    public class HsvColor {
        public HsvColor() {
        }

        public HsvColor(float h, float s, float v) {
            H = h;
            S = s;
            V = v;
        }

        public float H { get; set; }
        public float S { get; set; }
        public float V { get; set; }
    }
}
