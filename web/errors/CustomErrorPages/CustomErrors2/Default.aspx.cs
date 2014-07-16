using System;
using System.Web;

namespace AspNetResources.CustomErrors2
{
	/// <summary>
	/// Summary description for _Default.
	/// </summary>
	public class _Default : System.Web.UI.Page
	{
		private void Page_Load(object sender, System.EventArgs e)
		{
            // ------------------------------------------------------
            // No one is sane mind would throw an exception just for
            // the heck of it, but for demonstration purposes this
            // should work. Your event handler in Global.asax will
            // catch the exception.
            // ------------------------------------------------------
            throw new Exception ("Silly exception");
        }

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
