
namespace Yavsc.Models.Process
{
    public abstract class NamedRequisition : IRequisition
    {
        public string Name { get; set; }
        public abstract bool Eval();
    }
}