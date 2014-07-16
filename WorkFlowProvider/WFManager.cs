using System;
using yavscModel.WorkFlow;

namespace WorkFlowProvider
{
	public static class WFManager
	{
		public static IContentProvider GetContentProviderFWC ()
		{
			string clsName = System.Configuration.ConfigurationManager.AppSettings ["WorkflowContentProviderClass"];
			if (clsName == null)
				throw new Exception ("No content provider specified in the configuration file (Application parameter \"WorkflowContentProviderClass\")");
			System.Reflection.ConstructorInfo ci = Type.GetType (clsName).GetConstructor (System.Type.EmptyTypes);
			return (IContentProvider) ci.Invoke (System.Type.EmptyTypes);
		}
	}
}

