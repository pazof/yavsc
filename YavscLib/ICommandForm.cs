namespace YavscLib
{
    public interface ICommandForm
    {
         long Id { get; set; }
         string ActionName { get; set; }
         string Title { get; set; } 
         string ActivityCode { get; set; }
         
    }
}