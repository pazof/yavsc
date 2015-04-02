using System;
using SalesCatalog;
using System.Web.Routing;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Web.Http;
using System.Net.Http;
using System.Web;
using System.Linq;
using System.IO;
using System.Net;
using Yavsc;
using System.Web.Security;
using Yavsc.Model.WorkFlow;
using System.Reflection;
using System.Collections.Generic;
using Yavsc.Model.RolesAndMembers;
using Yavsc.Controllers;
using Yavsc.Formatters;
using System.Text;
using System.Web.Profile;
using System.Collections.Specialized;

namespace Yavsc.ApiControllers
{
	class TemplateException : Exception
	{
		public TemplateException(string message):base(message)
		{
		}
		public TemplateException(string message,Exception innerException):base(message,innerException)
		{
		}
	}

}

