using System;
using System.Xml.Serialization;
using Yavsc.Model.FrontOffice.Catalog;

namespace SalesCatalog.XmlImplementation
{
	/// <summary>
	/// Xml catalog.
	/// Inherits of the Catalog class,
	/// to make it serializable from and to Xml
	/// </summary>
	[XmlRoot]
	public class XmlCatalog : Catalog
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SalesCatalog.XmlImplementation.XmlCatalog"/> class.
		/// </summary>
		public XmlCatalog ()
		{
		}
	}
}

