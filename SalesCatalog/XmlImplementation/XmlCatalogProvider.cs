using System;
using System.Xml.Serialization;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Web;
using Yavsc.Model.FrontOffice.Catalog;

namespace SalesCatalog.XmlImplementation
{
	/// <summary>
	/// Xml catalog provider.
	/// In charge of getting the catalog data, 
	/// returning a Catalog object from GetCatalog
	/// </summary>
	public class XmlCatalogProvider: CatalogProvider
	{

		#region implemented abstract members of SalesCatalog.CatalogProvider
		/// <summary>
		/// Gets the catalog, loading it from
		/// the file system at a first call,
		/// and when its last write time has changed.
		/// </summary>
		/// <returns>The catalog.</returns>
		public override Catalog GetCatalog ()
		{
			// Assert fileName != null
			FileInfo fi = new FileInfo (fileName);
			if (!fi.Exists)
				throw new ConfigurationErrorsException(
					string.Format("No catalog found ({0})",fileName));
			if (fi.LastWriteTime > lastModification) 
				LoadCatalog ();
			return catInstance;
		}
		/// <summary>
		/// The catalog instance.
		/// </summary>
		protected XmlCatalog catInstance = null;
		/// <summary>
		/// The last modification date of the file loaded.
		/// </summary>
		protected DateTime lastModification = new DateTime(0);
		/// <summary>
		/// The name of the file loaded.
		/// </summary>
		protected string fileName = null;
		#endregion
		/// <summary>
		/// Initialize the catalog
		/// using the specified name and config.
		/// The config object contains under the key
		/// <c>connection</c> the path to the Xml Catalog file
		/// at server side.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="config">Config.</param>
		public override void Initialize (string name, System.Collections.Specialized.NameValueCollection config)
		{
			if (config ["connection"] == null)
				throw new Exception ("the 'connection' parameter is null " +
					"(it should be the absolute path to the xml catalog)");
			string cnx = (string) config ["connection"];
			fileName = HttpContext.Current.Server.MapPath(cnx);
			LoadCatalog ();
		}

		private void LoadCatalog ()
		{
			try {
			FileInfo fi = new FileInfo (fileName);
			if (!fi.Exists) 
				throw new Exception (
					string.Format ("Le fichier Xml decrivant le catalogue n'existe pas ({0})", fi.FullName));
			XmlSerializer xsr = new XmlSerializer (typeof(XmlCatalog),new Type[]{
				typeof(Service),
				typeof(PhysicalProduct),
				typeof(Euro),
				typeof(Text),
				typeof(TextInput),
				typeof(SelectInput)});

			using (FileStream fs = fi.OpenRead()) {
				catInstance = (XmlCatalog) xsr.Deserialize (fs);
			}
			fileName = fi.FullName;	
			lastModification = fi.LastWriteTime;
			}
			catch (Exception e) {
				lastModification = new DateTime (0);
				throw e; 
			}
		}
	}
}

