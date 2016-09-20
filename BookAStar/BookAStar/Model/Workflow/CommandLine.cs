
namespace BookAStar.Model.Workflow
{

    public class CommandLine
    {
        public long Id { get; set; }
        public string Comment { get; set; }
        public BaseProduct Article { get; set; }
        public int Count { get; set; }
        public decimal UnitaryCost { get; set; }
    }
}
