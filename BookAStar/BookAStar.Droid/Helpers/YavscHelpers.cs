using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Specialized;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using BookAStar.Model.Auth.Account;

namespace BookAStar.Droid
{

	public static class YavscHelpers
	{
		
        public static void SetRegId(this User user, string regId)
        {
            if (user.YavscTokens == null)
                throw new InvalidOperationException();

        }
		
	}
}

