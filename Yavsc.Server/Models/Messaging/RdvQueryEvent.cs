//
//  BookQueryEvent.cs
//
//  Author:
//       Paul Schneider <paul@pschneider.fr>
//
//  Copyright (c) 2015-2016 GNU GPL
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

namespace Yavsc.Models.Messaging
{
using Interfaces.Workflow;
    using Yavsc.Abstract.Messaging;

    public class RdvQueryEvent: RdvQueryProviderInfo, IEvent
	{
        public string SubTopic        
        {
            get; private set;
        }

        public RdvQueryEvent(string subTopic)
        {
             Topic = MessagingConstants.TopicRdvQuery;
             SubTopic = subTopic;
        }

        public string Sender
        {
            get; set;
        }

        public string Topic
        {
            get; private set;
        }

        public string CreateBody()
        {
            return string.Format(
                ResourcesHelpers.DefaultResourceManager.GetString("RdvToPerf"),
             Client.UserName,
            EventDate?.ToString("dddd dd/MM/yyyy à HH:mm"),
            Location.Address,
            ActivityCode);
        }
    }

}
