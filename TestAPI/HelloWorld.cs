//
//  HelloWorld.cs
//
//
// HelloWorld.cs
//
// Author:
//   Leonardo Taglialegne <leonardo.taglialegne@gmail.com>
//
// Copyright (c) 2013 Leonardo Taglialegne.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using NUnit.Framework;
using System.Net;
using Mono.WebServer.XSP;

namespace Mono.WebServer.Test
{


	[TestFixture]
	public class HelloWorld
	{

		[Test]
		public void TestCase ()
		{
			using (var server = new DebugServer()) {
				Assert.AreEqual (0, server.Run ());
				var wc = new WebClient ();
				try {
					string downloaded = wc.DownloadString ("http://localhost:8080/");
					//Assert.AreEqual (Environment.CurrentDirectory, downloaded);
					// ResponseHeaders	{
					// Date: Thu, 15 Oct 2015 16:12:00 GMT 
					// Server: Mono.WebServer.XSP/3.8.0.0 Linux 
					// X-AspNetMvc-Version: 3.0 
					// X-AspNet-Version: 4.0.30319 
					// Content-Length: 2180 
					// Cache-Control: private 
					// Content-Type: text/html 
					// Set-Cookie: ASP.NET_SessionId=ED208D636A4312B9745E396D; path=/ 
					// Keep-Alive: timeout=15, max=100 
					// Connection: Keep-Alive  }	System.Net.WebHeaderCollection 
					Assert.Greater(wc.ResponseHeaders["Set-Cookie"].Length, 10);

				} catch (WebException e) {
					Assert.Fail (e.Message);
				}
			}
		}
	}
}