using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspNetResources.CustomErrors4
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
            // should work.
            // ------------------------------------------------------
            //throw new Exception ("Silly exception");
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
