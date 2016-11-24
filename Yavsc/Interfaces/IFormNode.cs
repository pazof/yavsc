namespace Yavsc.Interfaces
{
    public interface IFormNode
    {
        
         T GetControl<T>();
         bool IsUIControl { get; }
         bool IsInputControl { get; }

    }
}