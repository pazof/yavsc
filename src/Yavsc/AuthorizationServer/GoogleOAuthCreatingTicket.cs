using System.Net.Http;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Authentication.OAuth;
using Microsoft.AspNet.Http;

namespace Yavsc.Auth {


    public class GoogleOAuthCreatingTicketContext : OAuthCreatingTicketContext {
        public GoogleOAuthCreatingTicketContext(HttpContext context, OAuthOptions options,
         HttpClient backchannel, OAuthTokenResponse tokens, AuthenticationTicket ticket, string googleUserId )
          : base( context, options, backchannel, tokens )
        {
            _ticket = ticket;
            _googleUserId = googleUserId;
            Principal = ticket.Principal;
        }

        readonly AuthenticationTicket _ticket;
        readonly string _googleUserId;

        public AuthenticationTicket Ticket { get { return _ticket; } }

        public string GoogleUserId { get { return _googleUserId; } }
    }


}
