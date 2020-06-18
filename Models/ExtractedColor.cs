namespace color_extraction_demo.Models {
    public class ExtractedColor {
        public HsvColor Hsv { get; set; }
        public string Hex { get; set; }
        public double Fraction { get; set; }
        public bool MatchedColor { get; set; }
    }
}
