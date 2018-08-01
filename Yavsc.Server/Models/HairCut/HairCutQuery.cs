
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Models.Billing;
using Yavsc.Models.Relationship;
using Yavsc.Billing;
using System.Globalization;
using Yavsc.Helpers;
using System.Linq;
using Microsoft.Extensions.Localization;
using Yavsc.ViewModels.PayPal;
using Yavsc.Models.HairCut;
using Yavsc.Abstract.Identity;

namespace Yavsc.Models.Haircut
{
    public class HairCutQuery : NominativeServiceCommand
    {

        // Bill description
        string _description = null;
        public override string Description
        {
            get
            {
                string type = ResourcesHelpers.GlobalLocalizer[this.GetType().Name];
                string gender = ResourcesHelpers.GlobalLocalizer[this.Prestation.Gender.ToString()];
                return $"{_description} {type} ({gender})";
            }
            set
            {
                _description = value;
            }
        }


        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        override public long Id { get; set; }

        [Required]
        public long PrestationId { get; set; }

        [ForeignKey("PrestationId"), Required, Display(Name = "Préstation")]
        public virtual HairPrestation Prestation { get; set; }

        [ForeignKey("LocationId")]
        [Display(Name = "Lieu du rendez-vous")]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "[Pas de lieu spécifié]")]
        public virtual Location Location { get; set; }

        [Display(Name = "Date et heure")]
        [DisplayFormat(NullDisplayText = "[Pas de date ni heure]", ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        public DateTime? EventDate
        {
            get;
            set;
        }

        public long? LocationId
        {
            get;

            set;
        }

        [Display(Name = "Informations complémentaires"),
        StringLengthAttribute(512)]
        [DisplayFormat(ConvertEmptyStringToNull = true, NullDisplayText = "[pas d'informations complémentaires]")]
        public string AdditionalInfo { get; set; }



        public override List<IBillItem> GetBillItems()
        {
            string longhairsuffix = " (cheveux longs)";
            string halflonghairsuffix = " (cheveux mi-longs)";
            string shorthairsuffix = " (cheveux courts)";

            List<IBillItem> bill = new List<IBillItem>();

            if (this.Prestation == null) throw new InvalidOperationException("Prestation property is null");
            if (this.SelectedProfile == null) throw new InvalidOperationException("SelectedProfile property is null");
            // Le shampoing
            if (this.Prestation.Shampoo)
                bill.Add(new CommandLine { Name = "Shampoing", Description = "Shampoing", UnitaryCost = SelectedProfile.ShampooPrice });

            // la coupe
            if (Prestation.Cut)
            {
                bill.Add(new CommandLine
                {

                    Name = "Coupe",
                    Description = $"Coupe " +
                   ResourcesHelpers.GlobalLocalizer[Prestation.Gender.ToString()] + " " +
                  (Prestation.Gender == HairCutGenders.Women ?
       Prestation.Length == HairLength.Long ? longhairsuffix :
       Prestation.Length == HairLength.HalfLong ? halflonghairsuffix :
       shorthairsuffix : null),
                    UnitaryCost =
Prestation.Gender == HairCutGenders.Women ?
       Prestation.Length == HairLength.Long ? SelectedProfile.WomenLongCutPrice :
       Prestation.Length == HairLength.HalfLong ? SelectedProfile.WomenHalfCutPrice :
       SelectedProfile.WomenShortCutPrice : Prestation.Gender == HairCutGenders.Man ?
       SelectedProfile.ManCutPrice : SelectedProfile.KidCutPrice
                });

            }

            // Les techniques
            switch (Prestation.Tech)
            {
                case HairTechnos.Color:
                    {
                        bool multicolor = Prestation.Taints.Count > 1;
                        string name = multicolor ? "Couleur" : "Multi-couleur";
                        switch (Prestation.Length)
                        {
                            case HairLength.Long:
                                bill.Add(new CommandLine
                                {
                                    Name = name,
                                    Description = name + longhairsuffix,
                                    UnitaryCost = multicolor ? SelectedProfile.LongMultiColorPrice :
                SelectedProfile.LongColorPrice
                                });
                                break;
                            case HairLength.HalfLong:
                                bill.Add(new CommandLine
                                {
                                    Name = name,
                                    Description = name + halflonghairsuffix,
                                    UnitaryCost = multicolor ? SelectedProfile.HalfMultiColorPrice : SelectedProfile.HalfColorPrice
                                });
                                break;
                            default:

                                bill.Add(new CommandLine
                                {
                                    Name = name,
                                    Description = name = name + shorthairsuffix,
                                    UnitaryCost = multicolor ? SelectedProfile.ShortMultiColorPrice : SelectedProfile.ShortColorPrice
                                });

                                break;
                        }
                    }
                    break;
                case HairTechnos.Balayage:
                    {
                        string name = "Balayage";
                        switch (Prestation.Length)
                        {
                            case HairLength.Long:
                                bill.Add(new CommandLine
                                {
                                    Name = name,
                                    Description = name + longhairsuffix,
                                    UnitaryCost = SelectedProfile.LongBalayagePrice
                                });
                                break;
                            case HairLength.HalfLong:
                                bill.Add(new CommandLine
                                {
                                    Name = name,
                                    Description = name + halflonghairsuffix,
                                    UnitaryCost = SelectedProfile.HalfBalayagePrice
                                });
                                break;
                            default:
                                bill.Add(new CommandLine
                                {
                                    Name = name,
                                    Description = name + shorthairsuffix,
                                    UnitaryCost = SelectedProfile.ShortBalayagePrice
                                });
                                break;
                        }
                    }
                    break;
                case HairTechnos.Defris:
                    {
                        string name = "Defrisage";
                        switch (Prestation.Length)
                        {
                            case HairLength.Long:
                                bill.Add(new CommandLine
                                {
                                    Name = name,
                                    Description = name + longhairsuffix,
                                    UnitaryCost = SelectedProfile.LongDefrisPrice
                                });
                                break;
                            case HairLength.HalfLong:
                                bill.Add(new CommandLine
                                {
                                    Name = name,
                                    Description = name + halflonghairsuffix,
                                    UnitaryCost = SelectedProfile.HalfDefrisPrice
                                });
                                break;
                            default:
                                bill.Add(new CommandLine
                                {
                                    Name = name,
                                    Description = name + shorthairsuffix,
                                    UnitaryCost = SelectedProfile.ShortDefrisPrice
                                });
                                break;
                        }
                    }
                    break;
                case HairTechnos.Mech:
                    {
                        string name = "Mèches";
                        switch (Prestation.Length)
                        {
                            case HairLength.Long:
                                bill.Add(new CommandLine
                                {
                                    Name = name,
                                    Description = name + longhairsuffix,
                                    UnitaryCost = SelectedProfile.LongMechPrice
                                });
                                break;
                            case HairLength.HalfLong:
                                bill.Add(new CommandLine
                                {
                                    Name = name,
                                    Description = name + halflonghairsuffix,
                                    UnitaryCost = SelectedProfile.HalfMechPrice
                                });
                                break;
                            default:
                                bill.Add(new CommandLine
                                {
                                    Name = name,
                                    Description = name + shorthairsuffix,
                                    UnitaryCost = SelectedProfile.ShortMechPrice
                                });
                                break;
                        }
                    }
                    break;
                case HairTechnos.Permanent:
                    {
                        string name = "Mèches";
                        switch (Prestation.Length)
                        {
                            case HairLength.Long:
                                bill.Add(new CommandLine
                                {
                                    Name = name,
                                    Description = name + longhairsuffix,
                                    UnitaryCost = SelectedProfile.LongPermanentPrice
                                });
                                break;
                            case HairLength.HalfLong:
                                bill.Add(new CommandLine
                                {
                                    Name = name,
                                    Description = name + halflonghairsuffix,
                                    UnitaryCost = SelectedProfile.HalfPermanentPrice
                                });
                                break;
                            default:
                                bill.Add(new CommandLine
                                {
                                    Name = name,
                                    Description = name + shorthairsuffix,
                                    UnitaryCost = SelectedProfile.ShortPermanentPrice
                                });
                                break;
                        }
                    }
                    break;
            }

            // Les coiffages
            switch (Prestation.Dressing)
            {
                case HairDressings.Brushing:
                    {
                        string name = "Brushing";


                        switch (Prestation.Gender)
                        {
                            case HairCutGenders.Women:
                                switch (Prestation.Length)
                                {
                                    case HairLength.Long:
                                        bill.Add(new CommandLine
                                        {
                                            Name = name,
                                            Description = name + longhairsuffix,
                                            UnitaryCost = SelectedProfile.LongBrushingPrice
                                        });
                                        break;
                                    case HairLength.HalfLong:
                                        bill.Add(new CommandLine
                                        {
                                            Name = name,
                                            Description = name + halflonghairsuffix,
                                            UnitaryCost = SelectedProfile.HalfBrushingPrice
                                        });
                                        break;
                                    default:
                                        bill.Add(new CommandLine
                                        {
                                            Name = name,
                                            Description = name + shorthairsuffix,
                                            UnitaryCost = SelectedProfile.ShortBrushingPrice
                                        });
                                        break;
                                }
                                break;
                            case HairCutGenders.Man:
                                bill.Add(new CommandLine
                                {
                                    Name = name,
                                    Description = name + shorthairsuffix,
                                    UnitaryCost = SelectedProfile.ManBrushPrice
                                });
                                break;
                        }
                    }
                    break;
                case HairDressings.Coiffage:
                    // est offert
                    /*           bill.Add(new CommandLine
                                   {
                                       Name = "Coiffage (offert)",
                                       UnitaryCost = 0m
                                   }); */
                    break;
                case HairDressings.Folding:
                    {
                        string name = "Mise en plis";

                        switch (Prestation.Length)
                        {
                            case HairLength.Long:
                                bill.Add(new CommandLine
                                {
                                    Name = name,
                                    Description = name + longhairsuffix,
                                    UnitaryCost = SelectedProfile.LongFoldingPrice
                                });
                                break;
                            case HairLength.HalfLong:
                                bill.Add(new CommandLine
                                {
                                    Name = name,
                                    Description = name + halflonghairsuffix,
                                    UnitaryCost = SelectedProfile.HalfFoldingPrice
                                });
                                break;
                            default:
                                bill.Add(new CommandLine
                                {
                                    Name = name,
                                    Description = name + shorthairsuffix,
                                    UnitaryCost = SelectedProfile.ShortFoldingPrice
                                });
                                break;
                        }
                    }
                    break;
            }

            // les soins
            if (Prestation.Cares)
            {
                bill.Add(new CommandLine
                {
                    Name = "Soins",
                    Description = "Soins",
                    UnitaryCost = SelectedProfile.CarePrice
                });

            }
            return bill;
        }

        public HairCutPayementEvent CreatePaymentEvent(PaymentInfo info, IStringLocalizer localizer)
        {

            return new HairCutPayementEvent(Client.UserName, info, this, localizer);
        }

        public virtual BrusherProfile SelectedProfile { get; set; }

        public HairCutQueryEvent CreateEvent(string subTopic, string reason, string sender)
        {

            string evdate = EventDate?.ToString("dddd dd/MM/yyyy à HH:mm") ?? "[pas de date spécifiée]";
            string address = Location?.Address ?? "[pas de lieu spécifié]";
            var p = Prestation;
            string total = GetBillItems().Addition().ToString("C", CultureInfo.CurrentUICulture);
            string strprestation = Description;
            string bill = string.Join("\n", GetBillItems().Select(
                l => $"{l.Name} {l.Description} {l.UnitaryCost} € " +
                ((l.Count != 1) ? "*" + l.Count.ToString() : "")));
            var yaev = new HairCutQueryEvent(subTopic)
            {
                Client = new ClientProviderInfo
                {
                    UserName = Client.UserName,
                    UserId = ClientId,
                    Avatar = Client.Avatar
                },
                Previsional = Previsional,
                EventDate = EventDate,
                Location = Location,
                Id = Id,
                ActivityCode = ActivityCode,
                Reason = reason,
                Sender = sender,
                BillingCode = BillingCodes.Brush
            };
            return yaev;
        }

    }
}
