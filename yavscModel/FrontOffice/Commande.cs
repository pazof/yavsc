using System;
using Yavsc;
using System.Collections.Specialized;
using Yavsc.Model.WorkFlow;
using Yavsc.Model.FileSystem;
using System.Web;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Yavsc.Model.WorkFlow;

namespace Yavsc.Model.FrontOffice
{
	/// <summary>
	/// Commande.
	/// </summary>
	public abstract class Command
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
		/// Gets or sets the name of the client.
		/// </summary>
		/// <value>The name of the client.</value>
		public string ClientName { get; set; }

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
				return UserFileSystemManager.GetFiles (Id.ToString());
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.FrontOffice.Command"/> class.
		/// </summary>
		public Command()
		{
		}

		/// <summary>
		/// Sets the parameters.
		/// </summary>
		/// <param name="collection">Collection.</param>
		public void SetParameters(Dictionary<string,string>  collection)
		{
			Parameters.Clear ();
			foreach (string key in collection.Keys) {
				if (key != "productref" && key != "type" && key != "clientname" ) {
					
					Parameters.Add (key, collection [key]);
					foreach (var prop in this.GetType().GetRuntimeProperties())
					{
						if (prop.Name == key && prop.CanWrite) {
							System.ComponentModel.TypeConverter tc = System.ComponentModel.TypeDescriptor.GetConverter(prop.PropertyType);
							prop.SetValue(this,tc.ConvertFromString(collection [key]));
						}
					}
				}
			}
		}

		/// <summary>
		/// Creates a command from the http post request.
		/// This methods applies to one product reference,
		/// given in a required value "ref" in the given 
		/// collection of name value couples.
		/// </summary>
		/// <param name="collection">Collection.</param>
		/// <param name="files">Files.</param>
		private CommandRegistration FromPost(Dictionary<string,string>  collection, NameObjectCollectionBase files)
		{
			// string catref=collection["catref"]; // Catalog Url from which formdata has been built
			ProductRef = collection ["productref"];
			if (ProductRef == null)
				throw new InvalidOperationException (
					"A product reference cannot be blank at command time");
			ClientName = collection ["clientname"];
			if (ClientName == null)
				throw new InvalidOperationException (
					"A client name cannot be blank at command time");

			CreationDate = DateTime.Now;
			Status = CommandStatus.Inserted;
			// stores the parameters:
			SetParameters(collection);
			var registration = WorkFlowManager.RegisterCommand (this); // gives a value to this.Id
			UserFileSystemManager.Put(Path.Combine("commandes",Id.ToString ()),files);
			return registration;
		}

		/// <summary>
		/// Creates a command using the specified collection
		/// as command parameters, handles the files upload,
		/// ans register the command in db, positionning the 
		/// command id.
		/// 
		/// Required values in the command parameters : 
		///
		/// * ref: the product reference,
		/// * type: the command concrete class name.
		///
		/// </summary>
		/// <returns>The command.</returns>
		/// <param name="collection">Collection.</param>
		/// <param name="files">Files.</param>
		/// <param name="cmdreg">Cmdreg.</param>
		public static Command CreateCommand (
			Dictionary<string,string> collection,
			NameObjectCollectionBase files, 
			out CommandRegistration cmdreg)
		{
			string type = collection ["type"];
			if (type == null)
				throw new InvalidOperationException (
				"A command type cannot be blank");
			var cmd = CreateCommand (type);
			cmdreg = cmd.FromPost (collection, files);
			return cmd;
		}

		/// <summary>
		/// Creates the command, for deserialisation,
		/// do not register it in database.
		/// </summary>
		/// <returns>The command.</returns>
		/// <param name="className">Class name.</param>
		public static Command CreateCommand (string className)
		{
			var type = Type.GetType (className);

			if (type == null)
				throw new InvalidOperationException (
				 "Cannot find the command class " + className);

			if (!typeof(Command).IsAssignableFrom(type))
				throw new InvalidOperationException (
					"No command is assignable from a " + className);
				
			ConstructorInfo ci = type.GetConstructor(new Type[]{});
			var cmd = ci.Invoke (new object[]{ });
			return cmd as Command;
		}

		/// <summary>
		/// Gets the command textual description.
		/// </summary>
		/// <returns>The description.</returns>
		public abstract string GetDescription ();
	}
}

