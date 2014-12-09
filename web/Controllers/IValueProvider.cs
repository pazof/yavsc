using System;
using Yavsc;
using SalesCatalog;
using SalesCatalog.Model;
using System.Web.Routing;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Web.Http;
using System.Net.Http;
using System.Web;
using System.Linq;
using System.IO;
using System.Net;
using WorkFlowProvider;
using System.Web.Security;
using Yavsc.Model.WorkFlow;
using System.Reflection;
using System.Collections.Generic;
using Yavsc.Model.RolesAndMembers;
using Yavsc.Controllers;
using Yavsc.Formatters;
using System.Text;
using System.Web.Profile;

namespace Yavsc.ApiControllers
{

	interface IValueProvider<T>
	{
		T GetValue();
	}

}

