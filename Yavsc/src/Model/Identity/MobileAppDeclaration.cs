using System.ComponentModel.DataAnnotations;

public class GoogleCloudMobileDeclaration {

    [Key]

    public string RegistrationId { get; set; }

    public string AuthToken { get; set; }

    public string AuthType { get; set; }

}
