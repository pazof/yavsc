using System;
using Yavsc.Model;
using System.Configuration;
using System.Text;
using System.Collections.Generic;

namespace fortune
{
	public class Entry {
		public string Message{ get; set; }
		public string Author{ get; set; }
		public string Body{ get; set; }
	}

	public class MyClass : IModule
	{
		public MyClass ()
		{
		}

		#region IModule implementation
		string tblname = "fortune";
		public void Install (System.Data.IDbConnection cnx)
		{
			using (var cmd = cnx.CreateCommand())
			{
				StringBuilder strb = new StringBuilder (string.Format("create table {0} ",tblname));
				strb.Append ( "( author character varying (1024) not null, \n");
				strb.Append ( "body character varying (65536) not null," +
					"CONSTRAINT uniqueid PRIMARY KEY (uniqueid)," +
					"uniqueid bigserial NOT NULL ) WITH (\n  OIDS=FALSE\n);"
				+string.Format("CREATE INDEX fki_cst{0}ref\n  ON wrtags\n  USING btree\n  (tagid);",
						tblname));
				cmd.CommandText = strb.ToString ();
				cmd.ExecuteNonQuery ();
			}
		}

		public void Uninstall (System.Data.IDbConnection cnx, bool removeConfig)
		{
			using (var cmd = cnx.CreateCommand ()) {
				cmd.CommandText = string.Format ("drop table {0};", tblname);
				cmd.ExecuteNonQuery ();
			}
		}


		public void Initialize (string name, System.Collections.Specialized.NameValueCollection config)
		{
	      	
		}
		/*
		public class Discovery { 
			IServiceProvider[] Provider;
			ISettingsProviderService[] SettingsBase; 
		}

		public Discovery GetServices() {
			throw new NotImplementedException ();
		}

		*/
		#endregion
	}
}

