//
//  PostJson.cs
//
//  Author:
//       Paul Schneider <paulschneider@free.fr>
//
//  Copyright (c) 2015 Paul Schneider
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Net;
using System.Text;
using System.IO;

namespace Yavsc.Helpers
{
	/// <summary>
	/// Simple json post method.
	/// </summary>
	public class SimpleJsonPostMethod<TQuery,TAnswer>: IDisposable
	{
		internal HttpWebRequest request = null;
		internal HttpWebRequest Request { get { return request; } }

		string CharSet  { 
			get { return Request.TransferEncoding; }
			set { Request.TransferEncoding=value;} 
		}
		string Method { get { return Request.Method; } }
		/// <summary>
		/// Gets the path.
		/// </summary>
		/// <value>The path.</value>
		public string Path { 
			get{ return Request.RequestUri.ToString(); } 
		}
		/// <summary>
		/// Sets the credential.
		/// </summary>
		/// <param name="cred">Cred.</param>
		public void SetCredential(string cred) {
			Request.Headers.Set(HttpRequestHeader.Authorization,cred);
		}

		/// <summary>
		/// Initializes a new instance of the Yavsc.Helpers.SimpleJsonPostMethod class.
		/// </summary>
		/// <param name="pathToMethod">Path to method.</param>
		public SimpleJsonPostMethod (string pathToMethod)
		{
			// ASSERT Request == null
			request = WebRequest.CreateHttp (pathToMethod);

			Request.Method = "POST";
			Request.Accept = "application/json";
			Request.ContentType = "application/json";
			Request.TransferEncoding = "UTF-8";
		}
		/// <summary>
		/// Invoke the specified query.
		/// </summary>
		/// <param name="query">Query.</param>
		public TAnswer Invoke(TQuery query)
		{
			
	//		DataContractJsonSerializer serquery = new DataContractJsonSerializer (typeof(TQuery));
		//	DataContractJsonSerializer seransw = new DataContractJsonSerializer (typeof(TAnswer));
			var cr = new Newtonsoft.Json.JsonSerializer ();
			using (MemoryStream streamQuery = new MemoryStream ()) {
				using (StreamWriter swr = new StreamWriter (streamQuery)) {
					cr.Serialize (swr, query);
				}
			}
			
			TAnswer ans = default (TAnswer);
			using (WebResponse response = Request.GetResponse ()) {
				using (Stream responseStream = response.GetResponseStream ()) {
					using (var rdr = new StreamReader (responseStream)) {
						ans = (TAnswer)cr.Deserialize (rdr, typeof(TAnswer));
					}
				}
				response.Close();
			}
			return ans;
		}

		#region IDisposable implementation

		/// <summary>
		/// Releases all resource used by the Yavsc.Helpers.SimpleJsonPostMethod object.
		/// </summary>
		public void Dispose ()
		{
			if (Request != null) Request.Abort ();
		}
		#endregion
	}
}

