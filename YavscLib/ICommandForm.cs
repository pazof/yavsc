namespace YavscLib
{
    public interface ICommandForm
    {
         long Id { get; set; }
         string Action { get; set; }
         string Title { get; set; } 
         string ActivityCode { get; set; }
         
    }
}