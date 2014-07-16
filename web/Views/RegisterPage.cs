using System;
using System.Web.UI.WebControls;
using yavscModel.RolesAndMembers;


namespace Yavsc
{
	public class RegisterPage : System.Web.Mvc.ViewPage<RegisterViewModel>
	{
		public RegisterPage ()
		{
		}

		public CreateUserWizard Createuserwizard1;

		public void OnRegisterSendMail(object sender, MailMessageEventArgs e)
 {
    // Set MailMessage fields.
    e.Message.IsBodyHtml = false;
    e.Message.Subject = "New user on Web site.";
    // Replace placeholder text in message body with information 
    // provided by the user.
			e.Message.Body = e.Message.Body.Replace("<%PasswordQuestion%>", Createuserwizard1.Question);
    e.Message.Body = e.Message.Body.Replace("<%PasswordAnswer%>",   Createuserwizard1.Answer);  
}
	}
}

