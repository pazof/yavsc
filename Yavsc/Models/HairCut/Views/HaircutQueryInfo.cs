
//
//  HaircutQueryInfo.cs
//
//  Author:
//       Paul Schneider <paulschneider@free.fr>
//
//  Copyright (c) 2015 - 2017 Paul Schneider
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
using System.ComponentModel.DataAnnotations;
using Yavsc.Models.Auth;
using Yavsc.Models.Relationship;

namespace Yavsc.Models.Haircut.Views
{
    public class HaircutQueryProviderInfo : HaircutQueryComonInfo {
        public HaircutQueryProviderInfo(HairCutQuery query) : base (query)
        {
            ClientInfo = new UserInfo(query.Client.Id, query.Client.UserName, query.Client.Avatar);
        }
        public UserInfo ClientInfo { get; set; }

    }
    public class HaircutQueryClientInfo : HaircutQueryComonInfo {
        public HaircutQueryClientInfo(HairCutQuery query) : base (query)
        {
            var user = query.PerformerProfile.Performer;
            ProviderInfo = new UserInfo(user.Id, user.UserName, user.Avatar);
        }
        public UserInfo ProviderInfo { get; set; }

    }
    public class HaircutQueryComonInfo
    {
        public HaircutQueryComonInfo(HairCutQuery query)
        {
            Id = query.Id;
            Prestation = query.Prestation;
            Status = query.Status;
            Location = query.Location;
            EventDate = query.EventDate;
            AdditionalInfo = query.AdditionalInfo;
        }
        public long Id { get; set; }
        public HairPrestation Prestation { get; set; }

        public QueryStatus Status { get; set; }

        public virtual Location Location { get; set; }
        public DateTime? EventDate
        {
            get;
            set;
        }

        [Display(Name="Informations complémentaires")]
        public string AdditionalInfo { get; set; }
    }
}
