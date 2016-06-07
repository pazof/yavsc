
public interface IApplication
{
    string ApplicationID { get; set; }
    string DisplayName { get; set; }
    string RedirectUri { get; set; }
    string LogoutRedirectUri { get; set; }
    string Secret { get; set; }
}
public interface IApplicationStore
{
    IApplication FindApplication(string clientId);

}