using System;
using System.Xml.Serialization;
using SalesCatalog.Model;
using System.Configuration;
using System.IO;
using System.Xml;

namespace SalesCatalog.XmlImplementation
{
	public class XmlCatalogProvider: CatalogProvider
	{
		#region implemented abstract members of SalesCatalog.CatalogProvider

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

		protected XmlCatalog catInstance = null;
		protected DateTime lastModification = new DateTime(0);
		protected string fileName = null;
		#endregion

		public override void Initialize (string name, System.Collections.Specialized.NameValueCollection config)
		{
			fileName = config ["connection"];
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

