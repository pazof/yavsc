//
//  ChatHub.cs
//
//  Author:
//       Paul Schneider <paul@pschneider.fr>
//
//  Copyright (c) 2016-2019 GNU GPL
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

using Microsoft.Extensions.Localization;

namespace Yavsc.Server.Hubs
{
    public class HubInputValidator {

        public IStringLocalizer _locator;
        public HubInputValidator(IStringLocalizer locator) {
            _locator = locator;
        }

        public Action<string,string,string> NotifyUser {get;set;}
        public bool ValidateRoomName (string roomName)
        {
            bool valid = ValidateStringLength(roomName,1,25);
            if (valid) valid = IsLetterOrDigit(roomName);
            if (!valid) NotifyUser(NotificationTypes.Error, "roomName", _locator.GetString("InvalidRoomName"));
            return valid;
        }
        public bool ValidateUserName (string userName)
        {
            bool valid = true;

            if (userName.Length<1 || userName[0] == '?' && userName.Length<2) valid = false;
            if (valid) {
                    string suname = (userName[0] == '?') ? userName.Substring(1) : userName;
                    if (valid) valid = ValidateStringLength(suname, 1,12);
                    if (valid) valid = IsLetterOrDigit(userName);
            }
            if (!valid)
                NotifyUser(
                    NotificationTypes.Error,
                    "userName",
                     _locator.GetString("InvalidUserName)"));
            return valid;
        }
        public bool ValidateMessage (string message)
        {
            if (!ValidateStringLength(message, 1, 10240))
            {
                NotifyUser(NotificationTypes.Error, "message", _locator.GetString("InvalidMessage"));
                return false;
            }
            return true;
        }
        public bool ValidateReason (string reason)
        {
            if (!ValidateStringLength(reason, 1,240))
            {
                NotifyUser(NotificationTypes.Error, "reason", "InvalidReason");
                return false;
            }
            return true;
        }
        static bool ValidateStringLength(string str, int minLen, int maxLen)
        {
            if (string.IsNullOrEmpty(str))
            {
                if (minLen<=0) {
                    return true;
                } else {
                    return false;
                }
            }
            if (str.Length>maxLen||str.Length<minLen) return false;
            return true;
        }
        static bool IsLetterOrDigit(string s)
        {
            foreach (var c in s)
                if (!char.IsLetterOrDigit(c))
                    return false;
            return true;
        }
    }
}
