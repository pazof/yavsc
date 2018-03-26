
namespace Yavsc.Models.Process
{
    public class Negation<Exp> : IRequisition where Exp : IRequisition
    {
        Exp _expression;
        public Negation(Exp expression)
        {
            _expression = expression;
        }
        public bool Eval()
        {
            return !_expression.Eval();
        }
    }
}
