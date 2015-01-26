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
			Request.Headers.Add(HttpRequestHeader.Authorization,cred);
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
		/// <summary>
		/// Releases all resource used by the <see cref="Yavsc.Helpers.SimpleJsonPostMethod`2"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="Yavsc.Helpers.SimpleJsonPostMethod`2"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="Yavsc.Helpers.SimpleJsonPostMethod`2"/> in an unusable state.
		/// After calling <see cref="Dispose"/>, you must release all references to the
		/// <see cref="Yavsc.Helpers.SimpleJsonPostMethod`2"/> so the garbage collector can reclaim the memory that the
		/// <see cref="Yavsc.Helpers.SimpleJsonPostMethod`2"/> was occupying.</remarks>
		public void Dispose ()
		{
			if (Request != null) Request.Abort ();
		}
		#endregion
	}
}

