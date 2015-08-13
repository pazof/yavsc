using System;
using Yavsc;
using System.Collections.Specialized;
using Yavsc.Model.WorkFlow;
using Yavsc.Model.FileSystem;
using System.Web;
using System.Collections.Generic;
using System.IO;


namespace Yavsc.Model.FrontOffice
{
	/// <summary>
	/// Commande.
	/// </summary>
	public class Command
	{
		/// <summary>
		/// Gets or sets the creation date.
		/// </summary>
		/// <value>The creation date.</value>
		public DateTime CreationDate { get; set; }

		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		public long Id { get; set; }

		/// <summary>
		/// Gets or sets the product reference.
		/// </summary>
		/// <value>The prod reference.</value>
		public CommandStatus Status { get; set; }

		/// <summary>
		/// Gets or sets the product reference.
		/// </summary>
		/// <value>The product reference.</value>
		public string ProductRef { get; set; }

		/// <summary>
		/// The parameters.
		/// </summary>
		public Dictionary<string,string> Parameters = new Dictionary<string,string> ();

		IEnumerable<FileInfo> Files { 
			get {
				return GetFSM().GetFiles (Id.ToString());
			}
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FrontOffice.Command"/> class.
		/// </summary>
		public Command()
		{
		}
		/// <summary>
		/// Froms the post.
		/// </summary>
		/// <param name="collection">Collection.</param>
		/// <param name="files">Files.</param>
		public void FromPost(NameValueCollection collection, NameObjectCollectionBase files)
		{
			// string catref=collection["catref"]; // Catalog Url from which formdata has been built
			ProductRef=collection["ref"]; // Required product reference
			CreationDate = DateTime.Now;
			Status = CommandStatus.Inserted;
			// stores the parameters:
			Parameters.Clear ();
			foreach (string key in collection.AllKeys) {
				if (key!="ref")
					Parameters.Add (key, collection [key]);
			}
			WorkFlowManager wfm = new WorkFlowManager ();
			wfm.RegisterCommand (this); // overrides this.Id
			string strcmdid = Id.ToString ();
			GetFSM().Put (strcmdid, files);
		}

		/// <summary>
		/// Creates a command using the specified collection
		/// as command parameters, handles the files upload.
		/// </summary>
		/// <param name="collection">Collection.</param>
		/// <param name="files">Files.</param>
		public Command (NameValueCollection collection, NameObjectCollectionBase files)
		{
			FromPost (collection, files);
		}

		private FileSystemManager GetFSM() {
			return new FileSystemManager ("~/commands/{0}");
		}
	}
}

