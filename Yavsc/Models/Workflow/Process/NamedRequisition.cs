
namespace Yavsc.Models
{
    public abstract class NamedRequisition : IRequisition
    {
        public string Name { get; set; }
        public abstract bool Eval();
    }
}