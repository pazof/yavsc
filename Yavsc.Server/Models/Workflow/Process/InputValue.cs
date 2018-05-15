
namespace Yavsc.Models.Process
{
    public class ConstInputValue : NamedRequisition
    {
        public bool Value { get; set; }
        public override bool Eval()
        {
            return Value;
        }
    }
}