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
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace Yavsc
{

    public class ChatHub : Hub
    {
        public override Task OnConnected()
        {
            if (Context.User!=null) {
            var group = (Context.User.Identity.IsAuthenticated)?
				"authenticated":"anonymous";
			// Log ("Cx: " + group);
			Groups.Add(Context.ConnectionId, group);
            } else Groups.Add(Context.ConnectionId, "anonymous");
			return base.OnConnected ();
        }
		public override Task OnDisconnected (bool stopCalled)
		{
			return base.OnDisconnected (stopCalled);
		}

    public override Task OnReconnected ()
		{
			return base.OnReconnected ();
		}

        public void Send(string name, string message)
        {

			Clients.All.addMessage(name,message);
        }

		[Authorize]
		public void AuthSend (string name, string message)
		{
			Clients.All.addMessage("#"+name,message);
		}

        [Authorize]
        public void PV (string userId, string message)
        {
            var sender = Context.User.Identity.Name;
            // TODO personal black|white list +
            // Contact list allowed only + 
            // only pro
            var hubCxContext = Clients.User(userId);
            var cli = Clients.Client(hubCxContext.ConnectionId);
            cli.addPV(sender,message);
        }
    }
}
