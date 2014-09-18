using System;
using Npgsql;
using NpgsqlTypes;
using System.Configuration;
using System.Collections.Specialized;
using yavscModel.WorkFlow;
using System.Web.Mvc;
using System.Configuration.Provider;

namespace WorkFlowProvider
{
	public class NpgsqlContentProvider: ProviderBase, IContentProvider
	{
		public Estimate GetEstimate (long estimid)
		{
			throw new NotImplementedException ();
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

		public bool[] FinalStatuses {
			get {
				throw new NotImplementedException ();
			}
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
				throw new NotImplementedException ();
			}
		}

		#region IDisposable implementation
		public void Dispose ()
		{

		}
		#endregion

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
	}
}

