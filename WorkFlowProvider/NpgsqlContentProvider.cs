using System;
using Npgsql;
using NpgsqlTypes;
using System.Configuration;
using System.Collections.Specialized;
using Yavsc.Model.WorkFlow;
using System.Configuration.Provider;
using System.Collections.Generic;
using Yavsc.Model.FrontOffice;
using Newtonsoft.Json;

namespace WorkFlowProvider
{
	public class NpgsqlContentProvider: ProviderBase, IContentProvider
	{
		public long RegisterCommand (Commande com)
		{
			long id;
			using (NpgsqlConnection cnx = CreateConnection ()) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = 
						"insert into commandes (prdref,creation,params) values (@pref,@creat,@prs) returning id";
					cmd.Parameters.Add ("@pref", com.ProdRef);
					cmd.Parameters.Add ("@creat", com.CreationDate);
					cmd.Parameters.Add ("@prs", JsonConvert.SerializeObject(com.Parameters));
					cnx.Open ();
					com.Id = id = (long)cmd.ExecuteScalar ();
				}
			}
			return id;
		}
		
		public StatusChange[] GetWrittingStatuses (long wrid)
		{
			throw new NotImplementedException ();
		}
		public StatusChange[] GetEstimateStatuses (long estid)
		{
			throw new NotImplementedException ();
		}
		public void TagWritting (long wrid, string tag)
		{
			throw new NotImplementedException ();
		}
		public void DropTagWritting (long wrid, string tag)
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
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}
		public void Install (System.Data.IDbConnection cnx)
		{
			throw new NotImplementedException ();
		}
		public void Uninstall (System.Data.IDbConnection cnx, bool removeConfig)
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

		public string[] Statuses {
			get {
				return new string[] { "Created", "Validated", "Success", "Error" };
			}
		}
		public bool[] FinalStatuses {
			get {
				return new bool[] { false, false, true, true };
			}
		}
		public Estimate[] GetEstimates (string client)
		{
			using (NpgsqlConnection cnx = CreateConnection ()) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = 
						"select _id from estimate where client = @clid";
					cmd.Parameters.Add ("@clid", client);
					cnx.Open ();
					List<Estimate> ests = new List<Estimate> ();
					using (NpgsqlDataReader rdr = cmd.ExecuteReader ()) {
						while (rdr.Read ()) {
							ests.Add(GetEstimate(rdr.GetInt64(0)));
						}
					}
					return ests.ToArray();
				}
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
						"select title,username,client,description from estimate where _id = @estid";

					cmd.Parameters.Add ("@estid", estimid);
					cnx.Open ();
					Estimate est = null;
					using (NpgsqlDataReader rdr = cmd.ExecuteReader ()) {
						if (!rdr.Read ()) {
							return null;
						}
						est = new Estimate ();
						est.Title = rdr.GetString(
							rdr.GetOrdinal("title"));
						est.Responsible = rdr.GetString(
							rdr.GetOrdinal("username"));
						est.Client = rdr.GetString (
							rdr.GetOrdinal ("client"));
						int index = rdr.GetOrdinal ("description"); 
						if (!rdr.IsDBNull (index))
							est.Description = rdr.GetString (index);
						est.Id = estimid;
						using (NpgsqlCommand cmdw = new NpgsqlCommand ("select _id, productid, ucost, count, description from writtings where estimid = @estid", cnx)) {
							cmdw.Parameters.Add("@estid", estimid);
							using (NpgsqlDataReader rdrw = cmdw.ExecuteReader ()) {
								List<Writting> lw = null; 
								if (rdrw.HasRows) {
									lw = new List<Writting> ();
									while (rdrw.Read ()) {
										Writting w = new Writting ();
										int dei = rdrw.GetOrdinal ("description");
										if (!rdrw.IsDBNull (dei))
											w.Description = rdrw.GetString (dei);
										int opi = rdrw.GetOrdinal ("productid");
										if (!rdrw.IsDBNull (opi))
											w.ProductReference = rdrw.GetString(opi);
										int oco = rdrw.GetOrdinal ("count");
										if (!rdrw.IsDBNull (oco))
											w.Count = rdrw.GetInt32 (oco);
										int ouc = rdrw.GetOrdinal ("ucost");
										if (!rdrw.IsDBNull (ouc))
											w.UnitaryCost = rdrw.GetDecimal (ouc);
										w.Id = rdrw.GetInt64 (rdrw.GetOrdinal ("_id"));
										lw.Add (w);
									}
									est.Lines = lw.ToArray ();
								}
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


		public void UpdateWritting (Writting wr)
		{
			using (NpgsqlConnection cnx = CreateConnection ()) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = 
						"update writtings set " +
						"description = @desc, " +
						"ucost = @ucost, " +
						"count = @count, " +
						"productid = @prdid " +
						"where _id = @wrid";
					cmd.Parameters.Add ("@wrid", wr.Id);
					cmd.Parameters.Add ("@desc", wr.Description);
					cmd.Parameters.Add ("@ucost", wr.UnitaryCost);
					cmd.Parameters.Add ("@prdid", wr.ProductReference);
					cmd.Parameters.Add ("@count", wr.Count);
					cnx.Open ();
					cmd.ExecuteNonQuery ();
					cnx.Close ();
				}
			}
		}
	
		public void UpdateEstimate (Estimate estim)
		{
			using (NpgsqlConnection cnx = CreateConnection ()) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = 
						"update estimate set title = @tit, username = @un, " +
						"description = @descr, client = @cli where _id = @estid";
					cmd.Parameters.Add ("@tit", estim.Title);
					cmd.Parameters.Add ("@un", estim.Responsible);
					cmd.Parameters.Add ("@descr", estim.Description);
					cmd.Parameters.Add ("@cli", estim.Client);
					cmd.Parameters.Add ("@estid", estim.Id);
					cnx.Open ();
					cmd.ExecuteNonQuery ();
					cnx.Close ();
				}
			}
		}

		public long Write (long estid, string desc, decimal ucost, int count, string productid)
		{
			using (NpgsqlConnection cnx = CreateConnection ()) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = 
						"insert into writtings (description, estimid, ucost, count, productid) VALUES (@dscr,@estid,@ucost,@count,@prdid) returning _id";
					cmd.Parameters.Add ("@dscr", desc);
					cmd.Parameters.Add("@estid", estid);

					cmd.Parameters.Add("@ucost", ucost);
					cmd.Parameters.Add("@count", count);
					cmd.Parameters.Add("@prdid", productid);
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


		public Estimate CreateEstimate (string responsible, string client, string title, string description)
		{
			using (NpgsqlConnection cnx = CreateConnection ()) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = 
							"insert into estimate (title,description,username,client,applicationname) " +
					"values (@tit,@descr,@resp,@un,@app) returning _id";
					cmd.Parameters.Add ("@tit", title);
					cmd.Parameters.Add ("@un", client);
					cmd.Parameters.Add ("@resp", responsible);
					cmd.Parameters.Add ("@descr", description);
					cmd.Parameters.Add("@app", ApplicationName);
					cnx.Open ();
					Estimate created = new Estimate ();
					created.Id = (long)cmd.ExecuteScalar ();
					cnx.Close ();
					created.Title = title;
					created.Description = description;
					created.Client = client;
					created.Responsible = responsible;
					return created;
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

	}
}

