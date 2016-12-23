using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Accounts;

namespace BookAStar.Droid.Accounts
{
    class YavscAccountAuthenticator : AbstractAccountAuthenticator
    {
        public YavscAccountAuthenticator(Context context): base(context)
        {

        }

        public override Bundle AddAccount(AccountAuthenticatorResponse response, string accountType, string authTokenType, string[] requiredFeatures, Bundle options)
        {
            throw new NotImplementedException();
        }

        public override Bundle ConfirmCredentials(AccountAuthenticatorResponse response, Account account, Bundle options)
        {
            throw new NotImplementedException();
        }

        public override Bundle EditProperties(AccountAuthenticatorResponse response, string accountType)
        {
            throw new NotImplementedException();
        }

        public override Bundle GetAuthToken(AccountAuthenticatorResponse response, Account account, string authTokenType, Bundle options)
        {
            throw new NotImplementedException();
        }

        public override string GetAuthTokenLabel(string authTokenType)
        {
            throw new NotImplementedException();
        }

        public override Bundle HasFeatures(AccountAuthenticatorResponse response, Account account, string[] features)
        {
            throw new NotImplementedException();
        }

        public override Bundle UpdateCredentials(AccountAuthenticatorResponse response, Account account, string authTokenType, Bundle options)
        {
            throw new NotImplementedException();
        }
    }
}