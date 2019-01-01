namespace Yavsc.Abstract.Workflow
{
    public interface IMayBeFixable
    {
         bool Fixable { get; }

         void TryAndFix();
    }
}