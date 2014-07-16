using NUnit.Framework;
using System;
using SalesCatalog.Model;

namespace SalesCatalog.Tests
{
	[TestFixture ()]
	public class TestBrands
	{
		[Test ()]
		public void TestCaseAddRemoveBrand ()
		{
			Catalog c = new Catalog ();
			c.Brands = new Brand[0];
			Brand b=c.AddBrand ("coko");
			if (c.Brands.Length != 1)
				throw new Exception ("Pas ajouté");
			if (b == null)
				throw new Exception ("Renvoyé null");
			if (b.Name != "coko")
				throw new Exception ("Pas le bon nom");
			if (c.Brands [0] != b)
				throw new Exception ("err index 0");
			if (c.GetBrand ("coko") != b)
				throw new Exception ("err get by name");
			if (!c.RemoveBrand ("coko"))
				throw new Exception ("Pas supprimé");
		}
	}
}

