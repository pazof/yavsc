using System;
using System.Web.UI.WebControls;
using Yavsc.Model.RolesAndMembers;


namespace Yavsc
{
	/// <summary>
	/// Register page.
	/// </summary>
	public class RegisterPage : System.Web.Mvc.ViewPage<RegisterViewModel>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.RegisterPage"/> class.
		/// </summary>
		public RegisterPage ()
		{
		}
		/// <summary>
		/// The createuserwizard1.
		/// </summary>
		public CreateUserWizard Createuserwizard1;
		/// <summary>
		/// Raises the register send mail event.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
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

