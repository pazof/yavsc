//
//  MyHub.cs
//
//  Author:
//       Paul Schneider <paul@pschneider.fr>
//
//  Copyright (c) 2016 GNU GPL
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
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Owin;
using Microsoft.Owin.Cors;
using System.Configuration;
using Microsoft.Owin.Security.DataProtection;
using System.Security.Cryptography;
using System.IO;
using Microsoft.Owin.Security.OAuth;
using System.Threading.Tasks;

namespace SignalRSelfHost
{

	[AuthorizeClaims(Users="paul")]
    public class MyHub : Hub
    {
		public override System.Threading.Tasks.Task OnConnected ()
		{
			/* var group = (Context.User.Identity.IsAuthenticated)?
				"authenticated":"anonymous";
			Console.WriteLine ("Cx: " + group);
			Groups.Add(Context.ConnectionId, group); */
			return base.OnConnected ();
		}
		public override System.Threading.Tasks.Task OnDisconnected (bool stopCalled)
		{
			return base.OnDisconnected (stopCalled);
		}
		public override System.Threading.Tasks.Task OnReconnected ()
		{
			return base.OnReconnected ();
		}

        public void Send(string name, string message)
        {
			Console.WriteLine (name+"> "+message);
			Clients.All.addMessage(name,message);

        }
		public void AuthSend (string message)
		{
			string name = Context.User.Identity.Name;
			Console.WriteLine (name+"# "+message);
			Clients.All.addMessage(name,message);
		}
    }
}