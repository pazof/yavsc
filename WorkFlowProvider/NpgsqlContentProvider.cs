using System;
using Npgsql;
using NpgsqlTypes;
using System.Configuration;
using System.Collections.Specialized;
using yavscModel.WorkFlow;
using System.Web.Mvc;

namespace WorkFlowProvider
{
	public class NpgsqlContentProvider:  IContentProvider
	{
		public IWFOrder CreateOrder ()
		{
			throw new NotImplementedException ();
		}
		public IWFOrder ImapctOrder (string orderid, FormCollection col)
		{
			throw new NotImplementedException ();
		}
		public bool[] IsFinalStatus {
			get {
				throw new NotImplementedException ();
			}
		}

		string applicationName=null;

		public string ApplicationName {
			get {
				return applicationName;
			}
		}

		string cnxstr = null;

		public NpgsqlContentProvider ()
		{
			Initialize("NpgsqlYavscContentProvider",ConfigurationManager.AppSettings);
		}

		public void Initialize (string name, NameValueCollection config)
		{
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

		#region IITContentProvider implementation

	
		#endregion

	}
}

