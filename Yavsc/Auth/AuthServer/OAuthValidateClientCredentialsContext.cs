

public class OAuthValidateClientCredentialsContext { 
    public OAuthValidateClientCredentialsContext(string clientId,string clientSecret,IApplicationStore applicationStore)
    {
        ClientId = clientId;
        ClientSecret = clientSecret;
        ApplicationStore = applicationStore;
    }
    public string ClientId { get; private set; }
    public string ClientSecret { get; private set; }
    public IApplicationStore ApplicationStore { get; private set; }

}