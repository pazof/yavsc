using System;
using Npgsql;
using NpgsqlTypes;
using System.Configuration;
using System.Collections.Specialized;
using Yavsc.Model.WorkFlow;
using System.Configuration.Provider;
using System.Collections.Generic;
using Yavsc.Model.FrontOffice;
using System.Web.Security;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Yavsc
{
	/// <summary>
	/// Npgsql content provider.
	/// </summary>
	public class NpgsqlContentProvider: ProviderBase, IContentProvider
	{
		

		/// <summary>
		/// Registers the command.
		/// </summary>
		/// <returns>The command id in db.</returns>
		/// <param name="com">COM.</param>
		public long RegisterCommand (Command com)
		{
			long id;

			using (NpgsqlConnection cnx = CreateConnection ()) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = 
						"insert into commandes (prdref,creation,params,clientname,applicationname) values (@pref,@creat,@prms,@cli,@app) returning id";
					cmd.Parameters.AddWithValue ("@pref", com.ProductRef);
					cmd.Parameters.AddWithValue ("@creat", com.CreationDate);
					cmd.Parameters.AddWithValue ("@prms", JsonConvert.SerializeObject(com.Parameters));
					cmd.Parameters.AddWithValue ("@cli", Membership.GetUser().UserName);
					cmd.Parameters.AddWithValue ("@app", ApplicationName);
					cnx.Open ();
					com.Id = id = (long)cmd.ExecuteScalar ();
				}
			}
			return id;
		}

		/// <summary>
		/// Gets the commands.
		/// </summary>
		/// <returns>The commands.</returns>
		/// <param name="username">Username.</param>
		public CommandSet GetCommands (string username )
		{
			// Check the user's authorisations
			MembershipUser user = Membership.GetUser ();
			if (user.UserName != username)
			if (!Roles.IsUserInRole ("Admin"))
			if (!Roles.IsUserInRole ("FrontOffice"))
				throw new Exception ("Not allowed");
			CommandSet cmds = new CommandSet ();

			using (NpgsqlConnection cnx = CreateConnection ()) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = 
						"select id,creation,prdref,params from commandes where @user = clientname and applicationname = @app";
					cmd.Parameters.AddWithValue ("@user", username);
					cmd.Parameters.AddWithValue ("@app", this.ApplicationName);
					cnx.Open ();
					using (NpgsqlDataReader rdr = cmd.ExecuteReader ()) {
						while (rdr.Read ()) {
							Command ycmd = new Command();
							ycmd.Id = rdr.GetInt64(0);
							ycmd.CreationDate = rdr.GetDateTime(1);
							ycmd.ProductRef = rdr.GetString(2);

							object prms = JsonConvert.DeserializeObject<Dictionary<string,string>>(rdr.GetString (3));
							ycmd.Parameters = prms as Dictionary<string,string> ;

							cmds.Add (ycmd);
						}
					}
				}
				cnx.Close ();
			}
			return cmds;
		}

		/// <summary>
		/// Gets the stock status.
		/// </summary>
		/// <returns>The stock status.</returns>
		/// <param name="productReference">Product reference.</param>
		public virtual StockStatus GetStockStatus (string productReference)
		{
			return StockStatus.NonExistent;
		}


		/// <summary>
		/// Gets the writting status changes.
		/// </summary>
		/// <returns>The writting statuses.</returns>
		/// <param name="wrid">Wrid.</param>
		public StatusChange[] GetWrittingStatuses (long wrid)
		{
			throw new NotImplementedException ();
		}
		/// <summary>
		/// Gets the estimate status changes.
		/// </summary>
		/// <returns>The estimate statuses.</returns>
		/// <param name="estid">Estid.</param>
		public StatusChange[] GetEstimateStatuses (long estid)
		{
			throw new NotImplementedException ();
		}
		/// <summary>
		/// Tags the writting.
		/// </summary>
		/// <param name="wrid">Wrid.</param>
		/// <param name="tag">Tag.</param>
		public void TagWritting (long wrid, string tag)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Drops the writting tag .
		/// </summary>
		/// <param name="wrid">Wrid.</param>
		/// <param name="tag">Tag.</param>
		public void DropWrittingTag (long wrid, string tag)
		{
			throw new NotImplementedException ();
		}
		/// <summary>
		/// Sets the writting status.
		/// </summary>
		/// <param name="wrtid">Wrtid.</param>
		/// <param name="status">Status.</param>
		/// <param name="username">Username.</param>
		public void SetWrittingStatus (long wrtid, int status, string username)
		{
			throw new NotImplementedException ();
		}
		/// <summary>
		/// Sets the estimate status.
		/// </summary>
		/// <param name="estid">Estid.</param>
		/// <param name="status">Status.</param>
		/// <param name="username">Username.</param>
		public void SetEstimateStatus (long estid, int status, string username)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Releases all resource used by the <see cref="Yavsc.NpgsqlContentProvider"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="Yavsc.NpgsqlContentProvider"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="Yavsc.NpgsqlContentProvider"/> in an unusable state. After
		/// calling <see cref="Dispose"/>, you must release all references to the <see cref="Yavsc.NpgsqlContentProvider"/> so
		/// the garbage collector can reclaim the memory that the <see cref="Yavsc.NpgsqlContentProvider"/> was occupying.</remarks>
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}
		/// <summary>
		/// Install the model in database using the specified cnx.
		/// </summary>
		/// <param name="cnx">Cnx.</param>
		public void Install (System.Data.IDbConnection cnx)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Uninstall the module data and data model from 
		/// database, using the specified connection.
		/// </summary>
		/// <param name="cnx">Cnx.</param>
		/// <param name="removeConfig">If set to <c>true</c> remove config.</param>
		public void Uninstall (System.Data.IDbConnection cnx, bool removeConfig)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Defaults the config.
		/// </summary>
		/// <returns>The config.</returns>
		/// <param name="appName">App name.</param>
		/// <param name="cnxStr">Cnx string.</param>
		public ConfigurationSection DefaultConfig (string appName, string cnxStr)
		{
			throw new NotImplementedException ();
		}


		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Yavsc.NpgsqlContentProvider"/> is active.
		/// </summary>
		/// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
		public bool Active {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		/// <summary>
		/// Gets the different status labels.
		/// 0 is the starting status. Each status is an integer and the 0-based index
		/// of a string in this array.
		/// </summary>
		/// <value>The status labels.</value>
		public string[] Statuses {
			get {
				return new string[] { "Created", "Validated", "Success", "Error" };
			}
		}

		/// <summary>
		/// Gets the final statuses.
		/// </summary>
		/// <value>The final statuses.</value>
		public bool[] FinalStatuses {
			get {
				return new bool[] { false, false, true, true };
			}
		}
		/// <summary>
		/// Gets the estimates created by 
		/// or for the given user by user name.
		/// </summary>
		/// <returns>The estimates.</returns>
		/// <param name="username">user name.</param>
		public Estimate[] GetEstimates (string username)
		{
			if (username == null)
				throw new InvalidOperationException (
					"username cannot be" +
					"  null at searching for estimates");
			List<long> ids = new List<long> ();
			List<Estimate> ests = new List<Estimate> ();
			using (NpgsqlConnection cnx = CreateConnection ()) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = 
						"select _id from estimate where client = @uname or username = @uname";
						
					cmd.Parameters.AddWithValue ("@uname", username);
					cnx.Open ();

					using (NpgsqlDataReader rdr = cmd.ExecuteReader ()) {
						while (rdr.Read ()) {
							ids.Add (rdr.GetInt64 (0));
						}
						rdr.Close ();
					}
					cnx.Close ();
				}
				foreach (long id in ids) 
					ests.Add(Get(id));
				return ests.ToArray();
			}
		}
		/// <summary>
		/// Gets the estimates.
		/// </summary>
		/// <returns>The estimates.</returns>
		/// <param name="client">Client.</param>
		/// <param name="responsible">Responsible.</param>
		public Estimate[] GetEstimates (string client, string responsible)
		{
			if (client == null && responsible == null)
				throw new InvalidOperationException (
					"client and responsible cannot be" +
					" both null at searching for estimates");
			List<long> ids = new List<long> ();
			List<Estimate> ests = new List<Estimate> ();
			using (NpgsqlConnection cnx = CreateConnection ()) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = 
						"select _id from estimate where ";

					if (client != null) {
						cmd.CommandText += "client = @clid";
						if (responsible != null)
							cmd.CommandText += " and ";
						cmd.Parameters.AddWithValue ("@clid", client);
						}
					if (responsible != null) {
						cmd.CommandText += "username = @resp";
						cmd.Parameters.AddWithValue ("@resp", responsible);
					}

					cnx.Open ();

					using (NpgsqlDataReader rdr = cmd.ExecuteReader ()) {
						while (rdr.Read ()) {
							ids.Add (rdr.GetInt64 (0));
						}
						rdr.Close ();
					}
					foreach (long id in ids) 
						ests.Add(Get(id));
					return ests.ToArray();
				}
			}
		}

		/// <summary>
		/// Drops the writting.
		/// </summary>
		/// <param name="wrid">Wrid.</param>
		public void DropWritting (long wrid)
		{

			using (NpgsqlConnection cnx = CreateConnection ()) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = 
						"delete from writtings where _id = @wrid";

					cmd.Parameters.AddWithValue ("@wrid", wrid);
					cnx.Open ();
					cmd.ExecuteNonQuery ();
				}
			}
		}

		/// <summary>
		/// Drops the estimate.
		/// </summary>
		/// <param name="estid">Estid.</param>
		public void DropEstimate (long estid)
		{
			using (NpgsqlConnection cnx = CreateConnection ()) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = 
						"delete from estimate where _id = @estid";

					cmd.Parameters.AddWithValue ("@estid", estid);
					cnx.Open ();
					cmd.ExecuteNonQuery ();
				}
			}
		}

		/// <summary>
		/// Gets the estimate by identifier.
		/// </summary>
		/// <returns>The estimate.</returns>
		/// <param name="estimid">Estimid.</param>
		public Estimate Get (long estimid)
		{
			Estimate est = null;
			using (NpgsqlConnection cnx = CreateConnection ()) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = 
						"select title,username,client,description from estimate where _id = @estid";

					cmd.Parameters.AddWithValue ("@estid", estimid);
					cnx.Open ();
					using (NpgsqlDataReader rdr = cmd.ExecuteReader ()) {
						if (!rdr.Read ()) {
							return null;
						}
						est = new Estimate ();
						est.Title = rdr.GetString(
							rdr.GetOrdinal("title"));

						est.Responsible = rdr.GetString(
							rdr.GetOrdinal("username"));
						int clientidx = rdr.GetOrdinal ("client");
						if (!rdr.IsDBNull (clientidx))
							est.Client = rdr.GetString (clientidx);
						int index = rdr.GetOrdinal ("description"); 
						if (!rdr.IsDBNull (index))
							est.Description = rdr.GetString (index);
						est.Id = estimid;

						rdr.Close ();
					}
				}
				// assert est != null
				using (NpgsqlCommand cmdw = new NpgsqlCommand ("select _id, productid, ucost, count, description from writtings where estimid = @estid", cnx)) {
					cmdw.Parameters.AddWithValue("@estid", estimid);
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
				return est;
			}
		}

		/// <summary>
		/// Updates the writting.
		/// </summary>
		/// <param name="wr">Wr.</param>
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
					cmd.Parameters.AddWithValue ("@wrid", wr.Id);
					cmd.Parameters.AddWithValue ("@desc", wr.Description);
					cmd.Parameters.AddWithValue ("@ucost", wr.UnitaryCost);
					cmd.Parameters.AddWithValue ("@prdid", wr.ProductReference);
					cmd.Parameters.AddWithValue ("@count", wr.Count);
					cnx.Open ();
					cmd.ExecuteNonQuery ();
					cnx.Close ();
				}
			}
		}
	
		/// <summary>
		/// Saves the given Estimate object in database.
		/// </summary>
		/// <param name="estim">the Estimate object.</param>
		public void Update (Estimate estim)
		{
			using (NpgsqlConnection cnx = CreateConnection ()) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = 
						"update estimate set title = @tit, username = @un, " +
						"description = @descr, client = @cli where _id = @estid";
					cmd.Parameters.AddWithValue ("@tit", estim.Title);
					cmd.Parameters.AddWithValue ("@un", estim.Responsible);
					cmd.Parameters.AddWithValue ("@descr", estim.Description);
					cmd.Parameters.AddWithValue ("@cli", estim.Client);
					cmd.Parameters.AddWithValue ("@estid", estim.Id);
					cnx.Open ();
					cmd.ExecuteNonQuery ();
					cnx.Close ();
				}
			}
		}

		/// <summary>
		/// Add a line to the specified estimate by id,
		/// using the specified desc, ucost, count and productid.
		/// </summary>
		/// <param name="estid">Estimate identifier.</param>
		/// <param name="desc">Textual description for this line.</param>
		/// <param name="ucost">Unitary cost.</param>
		/// <param name="count">Cost multiplier.</param>
		/// <param name="productid">Product identifier.</param>
		public long Write (long estid, string desc, decimal ucost, int count, string productid)
		{
			using (NpgsqlConnection cnx = CreateConnection ()) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = 
						"insert into writtings (description, estimid, ucost, count, productid) VALUES (@dscr,@estid,@ucost,@count,@prdid) returning _id";
					cmd.Parameters.AddWithValue ("@dscr", desc);
					cmd.Parameters.AddWithValue("@estid", estid);

					cmd.Parameters.AddWithValue("@ucost", ucost);
					cmd.Parameters.AddWithValue("@count", count);
					cmd.Parameters.AddWithValue("@prdid", productid);
					cnx.Open ();

					long res = (long) cmd.ExecuteScalar ();
					cnx.Close ();
					return res;
				}
			}
		}

		/// <summary>
		/// Sets the desc.
		/// </summary>
		/// <param name="writid">Writid.</param>
		/// <param name="newDesc">New desc.</param>
		public void SetDesc (long writid, string newDesc)
		{ 
			using (NpgsqlConnection cnx = CreateConnection ()) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = 
						"update writtings set description = @dscr where _id = @writid";
					cmd.Parameters.AddWithValue ("@tit", newDesc);
					cmd.Parameters.AddWithValue ("@writid", writid);
					cnx.Open ();
					cmd.ExecuteNonQuery ();
					cnx.Close ();
				}
			} 
		}

		/// <summary>
		/// Creates the estimate.
		/// </summary>
		/// <returns>The estimate.</returns>
		/// <param name="client">Client.</param>
		/// <param name="title">Title.</param>
		/// <param name="responsible">Responsible.</param>
		/// <param name="description">Description.</param>
		public Estimate CreateEstimate (string responsible, string client, string title, string description)
		{
			using (NpgsqlConnection cnx = CreateConnection ()) {
				using (NpgsqlCommand cmd = cnx.CreateCommand ()) {
					cmd.CommandText = 
							"insert into estimate (title,description,username,client,applicationname) " +
					"values (@tit,@descr,@resp,@un,@app) returning _id";
					cmd.Parameters.AddWithValue ("@tit", title);
					cmd.Parameters.AddWithValue ("@un", client);
					cmd.Parameters.AddWithValue ("@resp", responsible);
					cmd.Parameters.AddWithValue ("@descr", description);
					cmd.Parameters.AddWithValue("@app", ApplicationName);
					cnx.Open ();
					Estimate created = new Estimate ();
					created.Id = (long) cmd.ExecuteScalar ();
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
		/// <summary>
		/// Gets or sets the name of the application.
		/// </summary>
		/// <value>The name of the application.</value>
		public string ApplicationName {
			get {
				return applicationName;
			}
			set {
				applicationName = value;
			}
		}

		string cnxstr = null;
		/// <summary>
		/// Initialize this object using the specified name and config.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="config">Config.</param>
		public override void Initialize (string name, NameValueCollection config)
		{
			if ( string.IsNullOrWhiteSpace(config ["connectionStringName"]))
				throw new ConfigurationErrorsException ("No name for Npgsql connection string found");

			cnxstr = ConfigurationManager.ConnectionStrings [config ["connectionStringName"]].ConnectionString;
			applicationName = config["applicationName"] ?? "/";

		}

		/// <summary>
		/// Creates the connection.
		/// </summary>
		/// <returns>The connection.</returns>
		protected NpgsqlConnection CreateConnection ()
		{
			return new NpgsqlConnection (cnxstr);
		}

	}
}

