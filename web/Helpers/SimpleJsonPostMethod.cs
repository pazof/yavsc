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
using Newtonsoft.Json;

namespace Yavsc.Helpers
{
	public class SimpleJsonPostMethod<TQuery,TAnswer>: IDisposable
	{
		internal HttpWebRequest request = null;
		internal HttpWebRequest Request { get { return request; } }

		string CharSet  { 
			get { return Request.TransferEncoding; }
			set { Request.TransferEncoding=value;} 
		}
		string Method { get { return Request.Method; } }

		public string Path { 
			get{ return Request.RequestUri.ToString(); } 
		}

		public void SetCredential(string cred) {
			Request.Headers.Add(HttpRequestHeader.Authorization,cred);
		}

		public SimpleJsonPostMethod (string pathToMethod)
		{
			// ASSERT Request == null
			request = WebRequest.CreateHttp (pathToMethod);

			Request.Method = "POST";
			Request.Accept = "application/json";
			Request.ContentType = "application/json";
			Request.TransferEncoding = "UTF-8";
		}

		public TAnswer Invoke(TQuery query)
		{
			Byte[] bytes = System.Text.Encoding.UTF8.GetBytes (JsonConvert.SerializeObject(query));
			Request.ContentLength = bytes.Length;

			using (Stream dataStream = Request.GetRequestStream ()) {
				dataStream.Write (bytes, 0, bytes.Length);
				dataStream.Close ();
			}
			TAnswer ans = default (TAnswer);
			using (WebResponse response = Request.GetResponse ()) {
				using (Stream responseStream = response.GetResponseStream ()) {
					using (StreamReader readStream = new StreamReader (responseStream, Encoding.UTF8)) {
						string responseStr = readStream.ReadToEnd ();
						ans = JsonConvert.DeserializeObject<TAnswer> (responseStr);
						readStream.Close ();
					}
					responseStream.Close ();
				}
				response.Close();
			}
			return ans;
		}

		#region IDisposable implementation
		public void Dispose ()
		{
			if (Request != null) Request.Abort ();
		}
		#endregion
	}
}

