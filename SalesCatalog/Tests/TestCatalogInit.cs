#if TEST 

using System;
using NUnit.Framework;
using SalesCatalog.XmlImplementation;
using SalesCatalog.Model;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Text;

namespace SalesCatalog.Tests
{
	[TestFixture()]
	public class TestCatalogInit
	{
		[Test()]
		public void TestSerDeserCat ()
		{
			Catalog cat = new XmlCatalog ();
			Brand b = new Brand ();
			b.Logo = new ProductImage ();
			b.Logo.Src = "/images/dev.png";
			b.Logo.Alt = "Dev";
			b.Name = "Developpement à la carte";
			b.Slogan = "Votre logiciel, efficace, sûr, et sur mesure";
			ProductCategory si = new ProductCategory ();
			si.Name = "Systèmes d'information et sites Web";
			ProductCategory progiciel = new ProductCategory ();
			progiciel.Name = "Progiciels";
			b.Categories = new ProductCategory[]{ si, progiciel };
			Service simaint = new Service ();
			simaint.Name = "Maintenance logicielle";
			simaint.Description = "Correction des bugs, évolution";
			Service sidev = new Service ();
			sidev.Name = "Développement logiciel";
			sidev.Description = "Votre intranet, votre site Web, sur mesure, " +
				"développé en cycles courts, et en étroite collaboration avec vous";
			Service aubb = new Service ();
			aubb.Name = "Audit de sécurité en black box";
			aubb.Description = "Je recherche les failles de sécurité de votre SI ou site Web, depuis l'exterieur de " +
				"votre système, sans avoir eu connaissance d'aucun élément sur l'architécture de votre " +
				"système";
			Service auwb = new Service ();
			auwb.Name = "Audit de sécurité en white box";
			auwb.Description = "Je me déplace chez vous, pour travailler à partir de votre code source, " +
				"et isoler ses failles de sécurités";
			si.Products = new Product[] { simaint, sidev, aubb, auwb };
			Service maint = new Service ();
			maint.Name = "Maintenance logicielle";
			maint.Description = "Correction des bugs, évolution";
			Service dev = new Service ();
			dev.Name = "Développement logiciel";
			dev.Description = "Votre progiciel, sur mesure, " +
				"développé en cycles courts, et en étroite collaboration avec vous";
			progiciel.Products = new Product[] { maint, dev };
			SaleForm f = new SaleForm ();
			f.Action = "/testAction";
			TextInput ticat = new TextInput ("Choose a Title");
			ticat.Id = "title" ;
			ticat.MultiLine = true;
			SelectInput selSize = new SelectInput ();
			selSize.Id="size";
			Option o1 = new Option ();
			o1.Value = "1m"; o1.Text = "1 mois";
			Option o2 = new Option ();
			o2.Value = "2m"; o2.Text = "2 mois";
			Option o3 = new Option ();
			o3.Value = "6m"; o3.Text = "6 mois";
			selSize.Items = new Option [] { o1, o2, o3 };
			var txt1 = new SalesCatalog.Model.Text ();
			var txt2 = new SalesCatalog.Model.Text ();
			txt1.Val="Choose a title : ";
			txt2.Val = "[br]Choose the size : ";
			f.Items = new FormElement[] {txt1,ticat,txt2,selSize};
			b.DefaultForm = f;
			cat.Brands = new Brand[] { b };
			b.Categories = new ProductCategory[] { si, progiciel };
			XmlSerializer ser =
				new XmlSerializer 
				(typeof(XmlCatalog),
					new Type[]{typeof(Service),
						typeof(PhysicalProduct),
						typeof(Euro),
						typeof(TextInput),
						typeof(SalesCatalog.Model.Text),
						typeof(TextInput),
						typeof(SelectInput)
					});
			FileInfo fi = new FileInfo ("Catalog.xml");
			if (fi.Exists)
				fi.Delete ();
			using (FileStream ws = fi.OpenWrite()) {
				ser.Serialize (ws, cat);
			}
			using (FileStream rs = fi.OpenRead()) {
				using (XmlTextReader rdr = new XmlTextReader(rs)) {
					XmlCatalog copy = (XmlCatalog)ser.Deserialize (rdr);
					if (copy.Brands == null) throw new Exception("Null brand array!");
					if (copy.Brands.Length != cat.Brands.Length) throw new Exception("Not the same count of brands");
					if (copy.Brands[0].DefaultForm.Action != cat.Brands[0].DefaultForm.Action) throw new Exception("not the same default form");
					// ...
				}
			}
		}
	}
}

#endif
