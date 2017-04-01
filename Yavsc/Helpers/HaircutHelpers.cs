using Yavsc.Models.Haircut;

namespace Yavsc.Helpers
{
    public static class HaircutHelpers
    {
        public static decimal Addition (this HairPrestation p, BrusherProfile profile)
        {
            decimal sub=0;
            // Le shampoing
            sub += p.Shampoo ? profile.ShampooPrice:0;

            // la coupe
            sub += p.Cut ? p.Gender == HairCutGenders.Women ?
                        p.Length == HairLength.Long ? profile.WomenLongCutPrice :
                        p.Length == HairLength.HalfLong ? profile.WomenHalfCutPrice :
                        profile.WomenShortCutPrice : p.Gender == HairCutGenders.Man ?
                        profile.ManCutPrice : profile.KidCutPrice : 0;

            // Les techniques
            switch (p.Tech) {
                case HairTechnos.Color:
                    bool multicolor = p.Taints.Count>1;
                    switch (p.Length) {
                        case HairLength.Long:
                            sub += sub += multicolor? profile.LongMultiColorPrice : profile.LongColorPrice;
                            break;
                        case HairLength.HalfLong: sub += multicolor? profile.HalfMultiColorPrice : profile.HalfColorPrice;
                            break;
                        default:
                            sub +=  multicolor? profile.ShortMultiColorPrice : profile.ShortColorPrice;
                            break;
                    }
                    break;
                case HairTechnos.Balayage:
                    switch (p.Length) {
                        case HairLength.Long:
                            sub += profile.LongBalayagePrice;
                            break;
                        case HairLength.HalfLong: sub += profile.HalfBalayagePrice;
                            break;
                        default:
                            sub += profile.ShortBalayagePrice;
                            break;
                    }
                    break;
                case HairTechnos.Defris:
                    switch (p.Length) {
                        case HairLength.Long:
                            sub += profile.LongDefrisPrice;
                            break;
                        case HairLength.HalfLong: sub += profile.HalfDefrisPrice;
                            break;
                        default:
                            sub += profile.ShortDefrisPrice;
                            break;
                    }
                    break;
                case HairTechnos.Mech:
                    switch (p.Length) {
                        case HairLength.Long:
                            sub += profile.LongMechPrice;
                            break;
                        case HairLength.HalfLong: sub += profile.HalfMechPrice;
                            break;
                        default:
                            sub += profile.ShortMechPrice;
                            break;
                    }
                    break;
                case HairTechnos.Permanent:
                    switch (p.Length) {
                        case HairLength.Long:
                            sub += profile.LongPermanentPrice;
                            break;
                        case HairLength.HalfLong: sub += profile.HalfPermanentPrice;
                            break;
                        default:
                            sub += profile.ShortPermanentPrice;
                            break;
                    }
                    break;

            }

            // Les coiffages
            switch (p.Dressing) {
                case HairDressings.Brushing:
                    switch (p.Gender) {
                        case HairCutGenders.Women:
                            switch (p.Length) {
                                case HairLength.Long:
                                    sub += profile.LongBrushingPrice;
                                    break;
                                case HairLength.HalfLong: sub += profile.HalfBrushingPrice;
                                    break;
                                default:
                                    sub += profile.ShortBrushingPrice;
                                    break;
                            }
                            break;
                        case HairCutGenders.Man:
                            sub += profile.ManBrushPrice;
                            break;
                    }
                    break;
                case HairDressings.Coiffage:
                    // est offert
                    break;
                case HairDressings.Folding:
                     switch (p.Length) {
                        case HairLength.Long:
                            sub += profile.LongFoldingPrice;
                            break;
                        case HairLength.HalfLong: sub += profile.HalfFoldingPrice;
                            break;
                        default:
                            sub += profile.ShortFoldingPrice;
                            break;
                    }
                    break;
            }

            // les soins
            sub += p.Cares ? profile.CarePrice:0;
            return sub;

        }
    }
}
