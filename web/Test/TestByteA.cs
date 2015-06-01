#if TEST
using NUnit.Framework;
using System;
using System.Configuration;
using Npgsql;

namespace Yavsc
{
	[TestFixture ()]
	public class TestByteA: IDisposable
	{
		//string cnxName = "yavsc";
		string ConnectionString { get { 
				return "Server=127.0.0.1;Port=5432;Database=mae;User Id=mae;Password=admin;Encoding=Unicode;" ;
					//	return ConfigurationManager.ConnectionStrings [cnxName].ConnectionString; 
			
			} }

		[TestFixtureSetUp]
		public void Init()
		{
			// create the table
			Console.WriteLine ("cnx:"+ConnectionString);
			using (NpgsqlConnection cnx = new NpgsqlConnection (ConnectionString))
			{
				cnx.Open ();
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = "drop table testbytea";
					try { cmd.ExecuteNonQuery (); }
					catch (NpgsqlException) {
					}
					cmd.CommandText = "create table testbytea( t bytea )";
					cmd.ExecuteNonQuery ();
				}
			}
		}

		[Test(Description="Test storing a byte array in a Postgresql table field")]
		public void TestStoreByteA ()
		{
			byte []a = new byte[3];
			a[0]=1;
			a[1]=2;
			a[2]=3;
			using (NpgsqlConnection cnx = new NpgsqlConnection (ConnectionString)) {
				cnx.Open ();
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) 
				{
					cmd.CommandText = "insert into testbytea (t) values (@tv)";
					cmd.Parameters.AddWithValue ("@tv", a);
					cmd.ExecuteNonQuery ();
				}

				using (NpgsqlCommand cmd = cnx.CreateCommand ()) 
				{
					cmd.CommandText = "select t from testbytea";
					cmd.Parameters.AddWithValue ("@tv", a);

					NpgsqlDataReader rdr = cmd.ExecuteReader ();
					if (!rdr.Read ())
						throw new Exception ("Read failed");
					int i = rdr.GetOrdinal ("t");
					byte []rded = (byte[]) rdr.GetValue (i);
					if (rded.Length!=a.Length)
						throw new Exception("Lengthes don't match");
				}
			}
		}

		#region IDisposable implementation

		[TestFixtureTearDown]
		public void Dispose ()
		{
				// drop the table
			using (NpgsqlConnection cnx = new NpgsqlConnection (ConnectionString)) {
				cnx.Open ();
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = "drop table testbytea";
					cmd.ExecuteNonQuery ();
				}
			}
		}

		#endregion
	}
}

#endif