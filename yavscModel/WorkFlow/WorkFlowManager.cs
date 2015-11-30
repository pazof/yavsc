using System;
using Yavsc.Model.WorkFlow;
using System.Configuration;
using System.Collections.Specialized;
using Yavsc.Model.FrontOffice;
using System.Configuration.Provider;
using Yavsc.Model.FrontOffice.Catalog;
using System.Collections.Generic;
using Yavsc.Model.Skill;
using System.Linq;

namespace Yavsc.Model.WorkFlow
{
	
	/// <summary>
	/// Work flow manager.
	/// It takes orders store them and raise some events for modules
	/// It publishes estimates and invoices
	/// </summary>
	public static class WorkFlowManager 
	{

		/// <summary>
		/// Finds the activity.
		/// </summary>
		/// <returns>The activity.</returns>
		/// <param name="pattern">Pattern.</param>
		/// <param name="exerted">If set to <c>true</c> exerted.</param>
		public static Activity[] FindActivity(string pattern = "%", bool exerted=true)
		{
			List<Activity> activities = new List<Activity> ();
			foreach (var provider in Providers) {
				foreach (var act in provider.FindActivity (pattern, exerted))
					if (!activities.Contains(act))
						activities.Add(act);
			}
			return activities.ToArray();
		}

		/// <summary>
		/// Finds the performer.
		/// </summary>
		/// <returns>The performer.</returns>
		/// <param name="MEACode">MEA code.</param>
		/// <param name="skills">Skills.</param>
		public static PerformerProfile [] FindPerformer (string MEACode, SkillRating[] skills) 
		{
			string[] usernames = SkillManager.FindPerformer (MEACode, skills);
			List<PerformerProfile> result = new List<PerformerProfile> ();
			foreach (string user in usernames)
				result.Add (SkillManager.GetUserSkills (user));
			return result.ToArray ();
		}

		/// <summary>
		/// Gets the activity.
		/// </summary>
		/// <returns>The activity.</returns>
		/// <param name="meacode">MAE code.</param>
		public static Activity GetActivity (string meacode)
		{
			return DefaultProvider.GetActivity (meacode);
		}

		/// <summary>
		/// Gets or sets the catalog.
		/// </summary>
		/// <value>The catalog.</value>
		public static Catalog Catalog { get; set; }

		/// <summary>
		/// Registers the command.
		/// </summary>
		/// <returns>The command.</returns>
		/// <param name="com">COM.</param>
		public static long RegisterCommand(Command com)
		{
			return DefaultProvider.RegisterCommand (com);
		}

		/// <summary>
		/// Updates the estimate.
		/// </summary>
		/// <param name="estim">Estim.</param>
		public static void UpdateEstimate (Estimate estim)
		{
			DefaultProvider.Update (estim);
		}
		/// <summary>
		/// Gets the estimate.
		/// </summary>
		/// <returns>The estimate.</returns>
		/// <param name="estid">Estid.</param>
		public static Estimate GetEstimate (long estid)
		{
			return DefaultProvider.Get (estid);
		}
		/// <summary>
		/// Gets the estimates, refering the 
		/// given client or username .
		/// </summary>
		/// <returns>The estimates.</returns>
		/// <param name="responsible">Responsible.</param>
		public static Estimate [] GetResponsibleEstimates (string responsible)
		{
			return DefaultProvider.GetEstimates (null, responsible);
		}

		/// <summary>
		/// Gets the client estimates.
		/// </summary>
		/// <returns>The client estimates.</returns>
		/// <param name="client">Client.</param>
		public static Estimate [] GetClientEstimates (string client)
		{
			return DefaultProvider.GetEstimates (client, null);
		}

		/// <summary>
		/// Gets the user estimates.
		/// </summary>
		/// <returns>The user estimates.</returns>
		/// <param name="username">Username.</param>
		public static Estimate [] GetUserEstimates (string username)
		{
			return DefaultProvider.GetEstimates (username);
		}

		/// <summary>
		/// Gets the stock for a given product reference.
		/// </summary>
		/// <returns>The stock status.</returns>
		/// <param name="productReference">Product reference.</param>
		public static StockStatus GetStock(string productReference)
		{
			return DefaultProvider.GetStockStatus (productReference);
		}

		/// <summary>
		/// Updates the writting.
		/// </summary>
		/// <param name="wr">Wr.</param>
		public static void UpdateWritting (Writting wr)
		{
			DefaultProvider.UpdateWritting (wr);
		}

		/// <summary>
		/// Drops the writting.
		/// </summary>
		/// <param name="wrid">Wrid.</param>
		public static void DropWritting (long wrid)
		{
			DefaultProvider.DropWritting (wrid);
		}
		/// <summary>
		/// Drops the estimate.
		/// </summary>
		/// <param name="estid">Estid.</param>
		public static void DropEstimate (long estid)
		{
			DefaultProvider.DropEstimate(estid);
		}

		static IContentProvider defaultProvider;
		/// <summary>
		/// Gets the content provider.
		/// </summary>
		/// <value>The content provider.</value>
		public static IContentProvider DefaultProvider {
			
			get {
				if (defaultProvider == null)
					defaultProvider = ManagerHelper.CreateDefaultProvider
						("system.web/workflow") as IContentProvider; 
				return defaultProvider;
			}
		}
		/// <summary>
		/// Drops the writting tag.
		/// </summary>
		/// <param name="wrid">Wrid.</param>
		/// <param name="tag">Tag.</param>
		public static void DropWrittingTag (long wrid, string tag)
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Gets the providers.
		/// </summary>
		/// <value>The providers.</value>
		public static IContentProvider [] Providers {

			get { 
				if (providers == null) {
					var pbs = ManagerHelper.CreateProviders
						("system.web/workflow"); 
					providers = new IContentProvider [pbs.Length];
					for (var i=0;i<pbs.Length;i++)
						providers[i] = pbs[i] as IContentProvider;
				}
				return providers;
			}
		}

		private static IContentProvider [] providers = null;

		/// <summary>
		/// Creates the estimate.
		/// </summary>
		/// <returns>The estimate.</returns>
		/// <param name="responsible">Responsible.</param>
		/// <param name="client">Client.</param>
		/// <param name="title">Title.</param>
		/// <param name="description">Description.</param>
		public static Estimate CreateEstimate(string responsible, string client, string title, string description)
		{
			Estimate created = DefaultProvider.CreateEstimate (responsible, client, title, description);
			return created;
		}

		/// <summary>
		/// Write the specified estid, desc, ucost, count and productid.
		/// </summary>
		/// <param name="estid">Estid.</param>
		/// <param name="desc">Desc.</param>
		/// <param name="ucost">Ucost.</param>
		/// <param name="count">Count.</param>
		/// <param name="productid">Productid.</param>
		public static long Write(long estid, string desc, decimal ucost, int count, string productid)
		{
			if (!string.IsNullOrWhiteSpace(productid)) {
				if (Catalog == null)
					Catalog = CatalogManager.GetCatalog ();
				if (Catalog == null)
					throw new Exception ("No catalog");
				Product p = Catalog.FindProduct (productid);
				if (p == null)
					throw new Exception ("Product not found");
				// TODO new EstimateChange Event
			}
			return DefaultProvider.Write(estid, desc, ucost, count, productid);
		}

		/// <summary>
		/// Sets the estimate status.
		/// </summary>
		/// <param name="estid">Estid.</param>
		/// <param name="status">Status.</param>
		/// <param name="username">Username.</param>
		public static void SetEstimateStatus(long estid, int status, string username)
		{
			DefaultProvider.SetEstimateStatus (estid, status, username);
		}
		/// <summary>
		/// Gets the commands.
		/// </summary>
		/// <returns>The commands.</returns>
		/// <param name="username">Username.</param>
		public static CommandSet GetCommands(string username)
		{
			return DefaultProvider.GetCommands (username);
		}
		/// <summary>
		/// Registers the activity.
		/// </summary>
		/// <param name="activityName">Activity name.</param>
		/// <param name="meacode">Meacode.</param>
		/// <param name="comment">Comment.</param>
		public static void RegisterActivity (string activityName, string meacode, string comment)
		{
			DefaultProvider.RegisterActivity (activityName, meacode, comment);
		}
	}
}

