using System;
using yavscModel.WorkFlow;
using System.Configuration;
using WorkFlowProvider.Configuration;
using System.Collections.Specialized;

namespace WorkFlowProvider
{
	public static class WFManager
	{
		public static Estimate GetEstimate (long estid)
		{
			return ContentProvider.GetEstimate (estid);
		}

		public static void UpdateWritting (Writting wr)
		{
			ContentProvider.UpdateWritting (wr);
		}

		public static void DropWritting (long wrid)
		{
			ContentProvider.DropWritting (wrid);
		}
		public static void DropEstimate (long estid)
		{
			ContentProvider.DropEstimate(estid);
		}
		static IContentProvider contentProvider;

		public static IContentProvider ContentProvider {
			get {
				WorkflowConfiguration c = (WorkflowConfiguration) ConfigurationManager.GetSection ("system.web/workflow");
				if (c == null)
					throw new Exception ("No system.web/workflow configuration section found");
				WFProvider confprov = c.Providers.GetElement (c.DefaultProvider);
				if (confprov == null)
					throw new Exception ("Default workflow provider not found (system.web/workflow@defaultProvider)");
				string clsName = confprov.Type;
				if (clsName == null)
					throw new Exception ("Provider type not specified (system.web/workflow@type)");

				if (contentProvider != null)
				{ 
					if (contentProvider.GetType ().Name != clsName) 
						contentProvider = null;
				}
					
				if (contentProvider == null)
				{
					Type cpt = Type.GetType (clsName);
					if (cpt == null)
						throw new Exception (string.Format("Type not found : {0} (wrong name, or missing assembly reference?)",clsName));
					System.Reflection.ConstructorInfo ci =cpt.GetConstructor (System.Type.EmptyTypes);
					contentProvider = (IContentProvider)ci.Invoke (System.Type.EmptyTypes);
				}
					
				contentProvider.ApplicationName = confprov.ApplicationName;

				NameValueCollection config = new NameValueCollection ();
				config.Add ("name", confprov.Name);
				config.Add ("connectionStringName", confprov.ConnectionStringName);
				config.Add ("applicationName", confprov.ApplicationName);
				contentProvider.Initialize (confprov.Name, config);

				return contentProvider;


			}
		}

		/// <summary>
		/// Creates the estimate.
		/// </summary>
		/// <returns>The estimate identifier.</returns>
		/// <param name="title">Title.</param>
		public static long CreateEstimate(string client, string title)
		{
			return ContentProvider.CreateEstimate (client, title);
		}


		public static long Write(long estid, string desc, decimal ucost, int count, long productid)
		{
			return ContentProvider.Write(estid, desc, ucost, count, productid);
		}

		public static void SetEstimateStatus(long estid, int status, string username)
		{
			ContentProvider.SetEstimateStatus (estid, status, username);
		}

	}
}

