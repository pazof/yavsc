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
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNet.SignalR;

namespace Yavsc
{

  public class ChatHub : Hub
  {
    public override Task OnConnected()
    {
      bool isAuth = false;
      string userId = null;
      if (Context.User!=null) {
        isAuth = Context.User.Identity.IsAuthenticated;
        userId = Context.User.Identity.Name;
        var group = isAuth ?
          "authenticated":"anonymous";
        // Log ("Cx: " + group);
        Groups.Add(Context.ConnectionId, group);
      } else Groups.Add(Context.ConnectionId, "anonymous");

      Clients.Group("authenticated").notify("connected",  Context.ConnectionId, userId);
      return base.OnConnected ();
    }
    public override Task OnDisconnected (bool stopCalled)
    {
      string userId = Context.User?.Identity.Name;
      Clients.Group("authenticated").notify("disconnected",  Context.ConnectionId, userId);
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
      public void PV (string connectionId, string message)
      {
        var sender = Context.User.Identity.Name;
        // TODO personal black|white list +
        // Contact list allowed only + 
        // only pro
        var hubCxContext = Clients.User(connectionId);
        var cli = Clients.Client(connectionId);
        cli.addPV(sender,message);
      }
  }
}
