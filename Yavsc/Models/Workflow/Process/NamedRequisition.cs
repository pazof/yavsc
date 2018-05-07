using Yavsc.Interfaces;

namespace Yavsc.Models.Process
{
    public abstract class NamedRequisition : IRequisition, INamedObject
    {
        public string Name { get; set; }
        public abstract bool Eval();
    }
}
