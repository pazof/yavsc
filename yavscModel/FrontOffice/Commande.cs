using System;
using Yavsc;
using System.Collections.Specialized;
using Yavsc.Model.WorkFlow;
using Yavsc.Model.FileSystem;
using System.Web;


namespace Yavsc.Model.FrontOffice
{
	/// <summary>
	/// Commande.
	/// </summary>
	public class Commande
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
		public string ProductRef { get; set; }
		/// <summary>
		/// The parameters.
		/// </summary>
		public StringDictionary Parameters = new StringDictionary();

		FileInfoCollection Files { 
			get {
				return GetFSM().GetFiles (Id.ToString());
			}
		}

		/// <summary>
		/// Create a command using the specified collection
		/// as command parameters, handles the request files.
		/// </summary>
		/// <param name="collection">Collection.</param>
		/// <param name="files">Files.</param>
		public Commande (NameValueCollection collection, NameObjectCollectionBase files)
		{
			// string catref=collection["catref"]; // Catalog Url from which formdata has been built
			ProductRef=collection["ref"]; // Required product reference
			CreationDate = DateTime.Now;
			Status = CommandStatus.Inserted;
			// stores the parameters:
			foreach (string key in collection.AllKeys) {
				if (key!="ref")
					Parameters.Add (key, collection [key]);
			}
			WorkFlowManager wfm = new WorkFlowManager ();
			wfm.RegisterCommand (this); // sets this.Id
			string strcmdid = Id.ToString ();
			GetFSM().Put (strcmdid, files);
		}

		private FileSystemManager GetFSM() {
			return new FileSystemManager ("~/commands");
		}
	}
}

