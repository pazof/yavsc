

namespace Yavsc.Models {

    public class Parameter {
        public string Name { get; set; }
        public string Value { get; set; }
    }
    public interface IDocument {
        string Template { get; set; }
        Parameter [] Parameters { get; set; }
    }
}