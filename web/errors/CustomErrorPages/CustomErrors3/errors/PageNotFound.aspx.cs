using System;
using System.Web;

namespace AspNetResources.CustomErrors3
{
	/// <summary>
	/// Summary description for PageNotFound.
	/// </summary>
	public class PageNotFound : System.Web.UI.Page
	{
		private void Page_Load(object sender, System.EventArgs e)
		{
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
