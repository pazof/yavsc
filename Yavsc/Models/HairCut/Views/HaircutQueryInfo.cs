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
