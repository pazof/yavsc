using System.ComponentModel.DataAnnotations;
using YavscLib;

namespace Yavsc.Models.Haircut
{
    public class BrusherProfile : ISpecializationSettings
    {
        [Key]
        public string UserId
        {
           get; set;
        }

        /// <summary>
        /// StartOfTheDay In munutes
        /// </summary>
        /// <returns></returns>
        [Display(Name="Début de la journée")]
        public int StartOfTheDay { get; set;} 
        /// <summary>
        /// End Of The Day, In munutes
        /// </summary>
        /// <returns></returns>
        [Display(Name="Fin de la journée")]
        public int EndOfTheDay { get; set;}

        [Display(Name="Coût d'une coupe femme cheveux longs")]

        public decimal WomenLongCutPrice { get; set; }

        [Display(Name="Coût d'une coupe femme cheveux mi-longs")]
        public decimal WomenHalfCutPrice { get; set; }
        
        [Display(Name="Coût d'une coupe femme cheveux courts")]
        public decimal WomenShortCutPrice { get; set; }

        [Display(Name="Coût d'une coupe homme")]
        public decimal ManCutPrice { get; set; }

        [Display(Name="Coût d'une coupe enfant")]
        public decimal KidCutPrice { get; set; }

        [Display(Name="Coût d'un brushing cheveux longs")]
        public decimal LongBrushingPrice { get; set; }

        [Display(Name="Coût d'un brushing cheveux mi-longs")]
        public decimal HalfBrushingPrice { get; set; }

        [Display(Name="Coût d'un brushing cheveux courts")]
        public decimal ShortBrushingPrice { get; set; }

        [Display(Name="Coût du shamoing")]

        public decimal ShampooPrice { get; set; }

        [Display(Name="Coût du soin")]

        public decimal CarePrice { get; set; }

        [Display(Name="Coût d'une couleur cheveux longs")]
        public decimal LongColorPrice { get; set; }

        [Display(Name="Coût d'une couleur cheveux mi-longs")]
        public decimal HalfColorPrice { get; set; }

        [Display(Name="Coût d'une couleur cheveux courts")]
        public decimal ShortColorPrice { get; set; }

        [Display(Name="Coût d'une couleur multiple cheveux longs")]
        public decimal LongMultiColorPrice { get; set; }

        [Display(Name="Coût d'une couleur multiple cheveux mi-longs")]
        public decimal HalfMultiColorPrice { get; set; }

        [Display(Name="Coût d'une couleur multiple cheveux courts")]
        public decimal ShortMultiColorPrice { get; set; }

        [Display(Name="Coût d'une permanente cheveux longs")]
        public decimal LongPermanentPrice { get; set; }

        [Display(Name="Coût d'une permanente cheveux mi-longs")]
        public decimal HalfPermanentPrice { get; set; }

        [Display(Name="Coût d'une permanente cheveux courts")]
        public decimal ShortPermanentPrice { get; set; }

        [Display(Name="Coût d'un défrisage cheveux longs")]
        public decimal LongDefrisPrice { get; set; }

        [Display(Name="Coût d'un défrisage cheveux mi-longs")]
        public decimal HalfDefrisPrice { get; set; }

        [Display(Name="Coût d'un défrisage cheveux courts")]
        public decimal ShortDefrisPrice { get; set; }

        [Display(Name="Coût des mêches sur cheveux longs")]

        public decimal LongMechPrice { get; set; }

        [Display(Name="Coût des mêches sur cheveux mi-longs")]
        public decimal HalfMechPrice { get; set; }

        [Display(Name="Coût des mêches sur cheveux courts")]
        public decimal ShortMechPrice { get; set; }

        [Display(Name="Coût du balayage cheveux longs")]

        public decimal LongBalayagePrice { get; set; }

        [Display(Name="Coût du balayage cheveux mi-longs")]
        public decimal HalfBalayagePrice { get; set; }

        [Display(Name="Coût du balayage cheveux courts")]
        public decimal ShortBalayagePrice { get; set; }
        

    }
}