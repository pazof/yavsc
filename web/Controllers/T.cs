using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Net.Mail;
using Yavsc;
using System.Globalization;

namespace Yavsc
{
	public class T
	{
		public static string GetString(string msgid)
		{
			return Mono.Unix.Catalog.GetString (msgid);
		}

	}
}
