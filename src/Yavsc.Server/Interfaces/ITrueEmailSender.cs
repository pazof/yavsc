
namespace Yavsc.Interface 
{
public interface ITrueEmailSender
{
    //
    // Résumé :
    //     This API supports the ASP.NET Core Identity default UI infrastructure and is
    //     not intended to be used directly from your code. This API may change or be removed
    //     in future releases.
    Task<string> SendEmailAsync(string name, string email, string subject, string htmlMessage);
}


}
