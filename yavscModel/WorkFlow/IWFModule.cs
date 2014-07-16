using System;
using yavscModel.WorkFlow;
using System.Web.Mvc;

namespace WorkFlow
{
	public interface IWFModule
	{
		int GetState (IWFCommand c);
		int Handle (IWFCommand c,FormCollection collection);
	}
}

