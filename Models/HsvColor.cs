namespace color_extraction_demo.Models {
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
