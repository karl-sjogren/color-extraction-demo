using System.Collections.Generic;

namespace color_extraction_demo.Models {
    public class ImageColors {
        public string Filename { get; set; }
        public List<ExtractedColor> Colors { get; set; }
    }
}
