using System;
using Npgsql;
using NpgsqlTypes;
using System.Configuration;
using System.Collections.Specialized;
using Yavsc.Model.WorkFlow;
using System.Web.Mvc;
using System.Configuration.Provider;
using System.Collections.Generic;

namespace WorkFlowProvider
{
	public class NpgsqlContentProvider: ProviderBase, IContentProvider
	{
		public Estimate[] GetEstimates (string client)
		{
			throw new NotImplementedException ();
		}

		public void Install ()
		{
			throw new NotImplementedException ();
		}

		public void Uninstall ()
		{
			throw new NotImplementedException ();
		}

		public ConfigurationSection DefaultConfig (string appName, string cnxStr)
		{
			throw new NotImplementedException ();
		}

		public bool Active {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public StatusChange[] GetWrittingStatuses (long wrid)
		{
			throw new NotImplementedException ();
		}

		public StatusChange[] GetEstimateStatuses (long estid)
		{
			throw new NotImplementedException ();
		}

		public void DropTagWritting (long wrid, string tag)
		{
			throw new NotImplementedException ();
		}

		public void UpdateWritting (Writting wr)
		{
			throw new NotImplementedException ();
		}

		public void SetWrittingStatus (long wrtid, int status, string username)
		{
			throw new NotImplementedException ();
		}

		public void SetEstimateStatus (long estid, int status, string username)
		{
			throw new NotImplementedException ();
		}

		public void TagWritting (long wrid, string tag)
		{
			throw new NotImplementedException ();
		}



		public string Order (IWFOrder c)
		{
			throw new NotImplementedException ();
		}

		public IContent GetBlob (string orderId)
		{
			throw new NotImplementedException ();
		}

		public int GetStatus (string orderId)
		{
			throw new NotImplementedException ();
		}

		public string[] StatusLabels {
			get {
				return new string[] { "Created", "Validated", "Success", "Error" };
			}
		}
		public bool[] FinalStatuses {
			get {
				return new bool[] { false, false, true, true };
			}
		}

		public void DropWritting (long wrid)
		{

			using (NpgsqlConnection cnx = CreateConnection ()) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = 
						"delete from writtings where _id = @wrid";

					cmd.Parameters.Add ("@wrid", wrid);
					cnx.Open ();
					cmd.ExecuteNonQuery ();
				}
			}
		}

		public void DropEstimate (long estid)
		{
			using (NpgsqlConnection cnx = CreateConnection ()) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = 
						"delete from estimate where _id = @estid";

					cmd.Parameters.Add ("@estid", estid);
					cnx.Open ();
					cmd.ExecuteNonQuery ();
				}
			}
		}

		public Estimate GetEstimate (long estimid)
		{
			using (NpgsqlConnection cnx = CreateConnection ()) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = 
						"select _id,title,username from estimate where _id = @estid";

					cmd.Parameters.Add ("@estid", estimid);
					cnx.Open ();
					Estimate est = null;
					using (NpgsqlDataReader rdr = cmd.ExecuteReader ()) {
						if (!rdr.Read ()) {
							throw new Exception (
								string.Format("Estimate not found : {0}", estimid));
						}
						est = new Estimate ();
						est.Title = rdr.GetString(
							rdr.GetOrdinal("title"));
						est.Owner = rdr.GetString(
							rdr.GetOrdinal("username"));

						using (NpgsqlCommand cmdw = new NpgsqlCommand ("select _id, productid, ucost, count, description from writtings where _id = @estid", cnx)) {
							cmdw.Parameters.Add("@estid", estimid);
							using (NpgsqlDataReader rdrw = cmdw.ExecuteReader ()) {
								List<Writting> lw = new List<Writting> ();
								while (rdrw.Read ()) {
									Writting w = new Writting ();
									w.Description = rdrw.GetString (
										rdrw.GetOrdinal ("description"));
									int opi = rdrw.GetOrdinal ("productid");
									if (!rdrw.IsDBNull(opi))				
										w.ProductReference = rdrw.GetInt64(opi);
									int oco = rdrw.GetOrdinal ("count");
									if (!rdrw.IsDBNull(oco))
										w.Count = rdrw.GetInt32 (oco);
									int ouc = rdrw.GetOrdinal ("ucost");
									if (!rdrw.IsDBNull(ouc))
										w.UnitaryCost = rdrw.GetDecimal (ouc);
									w.Id = rdrw.GetInt64 (rdrw.GetOrdinal("_id"));
									lw.Add (w);
								}
								est.Lines = lw.ToArray ();
							}
						}
						// TODO est.Ciffer = somme des ecritures
						// TODO read into est.Lines 
					}
					cnx.Close ();
					return est;
				}
			}
		}
	
		public void SetTitle (long estid, string newTitle)
		{
			using (NpgsqlConnection cnx = CreateConnection ()) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = 
						"update estimate set title = @tit where _id = @estid";
					cmd.Parameters.Add ("@tit", newTitle);
					cmd.Parameters.Add ("@estid", estid);
					cnx.Open ();
					cmd.ExecuteNonQuery ();
					cnx.Close ();
				}
			}
		}

		public long Write (long estid, string desc, decimal ucost, int count, long productid)
		{
			using (NpgsqlConnection cnx = CreateConnection ()) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = 
						"insert into writtings (description, estimid) VALUES (@dscr,@estid) returning _id";
					cmd.Parameters.Add ("@dscr", desc);
					// cmd.Parameters.Add ("@prdid", productid);
					// cmd.Parameters.Add("@ucost", ucost);
					// cmd.Parameters.Add("@mult", count);
					cmd.Parameters.Add("@estid", estid);
					cnx.Open ();

					long res = (long) cmd.ExecuteScalar ();
					cnx.Close ();
					return res;
				}
			}
		}

		public void SetDesc (long writid, string newDesc)
		{ 
			using (NpgsqlConnection cnx = CreateConnection ()) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = 
						"update writtings set description = @dscr where _id = @writid";
					cmd.Parameters.Add ("@tit", newDesc);
					cmd.Parameters.Add ("@writid", writid);
					cnx.Open ();
					cmd.ExecuteNonQuery ();
					cnx.Close ();
				}
			} 
		}


		public long CreateEstimate (string client, string title)
		{
			using (NpgsqlConnection cnx = CreateConnection ()) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = 
							"insert into estimate (title,username,applicationname) " +
					"values (@tit,@un,@app) returning _id";
					cmd.Parameters.Add ("@tit", title);
					cmd.Parameters.Add ("@un", client);
					cmd.Parameters.Add("@app", ApplicationName);
					cnx.Open ();
					long res = (long)cmd.ExecuteScalar ();
					cnx.Close ();
					return res;
				}
			}
		}

		string applicationName=null;

		public string ApplicationName {
			get {
				return applicationName;
			}
			set {
				applicationName = value;
			}
		}

		string cnxstr = null;

		public override void Initialize (string name, NameValueCollection config)
		{
			if ( string.IsNullOrWhiteSpace(config ["connectionStringName"]))
				throw new ConfigurationErrorsException ("No name for Npgsql connection string found");

			cnxstr = ConfigurationManager.ConnectionStrings [config ["connectionStringName"]].ConnectionString;
			applicationName = config["applicationName"] ?? "/";



		}

		protected NpgsqlConnection CreateConnection ()
		{
			return new NpgsqlConnection (cnxstr);
		}
		#region IDisposable implementation
		public void Dispose ()
		{

		}
		#endregion

	}
}

