using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Yavsc.Models.Workflow;
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

        [JsonIgnore,ForeignKey("UserId")]
        public virtual PerformerProfile BaseProfile { get; set; }


        [Display(Name="Rayon d'action"),DisplayFormat(DataFormatString="{0} km")]

        public int ActionDistance { get; set; }
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

        [Display(Name="Coupe femme cheveux longs"),DisplayFormat(DataFormatString="{0:C}")]

        public decimal WomenLongCutPrice { get; set; }

        [Display(Name="Coupe femme cheveux mi-longs"),DisplayFormat(DataFormatString="{0:C}")]
        public decimal WomenHalfCutPrice { get; set; }

        [Display(Name="Coupe femme cheveux courts"),DisplayFormat(DataFormatString="{0:C}")]
        public decimal WomenShortCutPrice { get; set; }

        [Display(Name="Coupe homme"),DisplayFormat(DataFormatString="{0:C}")]
        public decimal ManCutPrice { get; set; }

        [Display(Name="brushing homme"),DisplayFormat(DataFormatString="{0:C}")]
        public decimal ManBrushPrice { get; set; }



        [Display(Name="Coupe enfant"),DisplayFormat(DataFormatString="{0:C}")]
        public decimal KidCutPrice { get; set; }

        [Display(Name="Brushing cheveux longs"),DisplayFormat(DataFormatString="{0:C}")]
        public decimal LongBrushingPrice { get; set; }

        [Display(Name="Brushing cheveux mi-longs"),DisplayFormat(DataFormatString="{0:C}")]
        public decimal HalfBrushingPrice { get; set; }

        [Display(Name="Brushing cheveux courts"),DisplayFormat(DataFormatString="{0:C}")]
        public decimal ShortBrushingPrice { get; set; }

        [Display(Name="couleur cheveux longs"),DisplayFormat(DataFormatString="{0:C}")]
        public decimal LongColorPrice { get; set; }

        [Display(Name="couleur cheveux mi-longs"),DisplayFormat(DataFormatString="{0:C}")]
        public decimal HalfColorPrice { get; set; }

        [Display(Name="couleur cheveux courts"),DisplayFormat(DataFormatString="{0:C}")]
        public decimal ShortColorPrice { get; set; }

        [Display(Name="couleur multiple cheveux longs"),DisplayFormat(DataFormatString="{0:C}")]
        public decimal LongMultiColorPrice { get; set; }

        [Display(Name="couleur multiple cheveux mi-longs"),DisplayFormat(DataFormatString="{0:C}")]
        public decimal HalfMultiColorPrice { get; set; }

        [Display(Name="couleur multiple cheveux courts"),DisplayFormat(DataFormatString="{0:C}")]
        public decimal ShortMultiColorPrice { get; set; }

        [Display(Name="permanente cheveux longs"),DisplayFormat(DataFormatString="{0:C}")]
        public decimal LongPermanentPrice { get; set; }

        [Display(Name="permanente cheveux mi-longs"),DisplayFormat(DataFormatString="{0:C}")]
        public decimal HalfPermanentPrice { get; set; }

        [Display(Name="permanente cheveux courts"),DisplayFormat(DataFormatString="{0:C}")]
        public decimal ShortPermanentPrice { get; set; }

        [Display(Name="défrisage cheveux longs"),DisplayFormat(DataFormatString="{0:C}")]
        public decimal LongDefrisPrice { get; set; }

        [Display(Name="défrisage cheveux mi-longs"),DisplayFormat(DataFormatString="{0:C}")]
        public decimal HalfDefrisPrice { get; set; }

        [Display(Name="défrisage cheveux courts"),DisplayFormat(DataFormatString="{0:C}")]
        public decimal ShortDefrisPrice { get; set; }

        [Display(Name="mêches sur cheveux longs"),DisplayFormat(DataFormatString="{0:C}")]

        public decimal LongMechPrice { get; set; }

        [Display(Name="mêches sur cheveux mi-longs"),DisplayFormat(DataFormatString="{0:C}")]
        public decimal HalfMechPrice { get; set; }

        [Display(Name="mêches sur cheveux courts"),DisplayFormat(DataFormatString="{0:C}")]
        public decimal ShortMechPrice { get; set; }

        [Display(Name="balayage cheveux longs"),DisplayFormat(DataFormatString="{0:C}")]

        public decimal LongBalayagePrice { get; set; }

        [Display(Name="balayage cheveux mi-longs"),DisplayFormat(DataFormatString="{0:C}")]
        public decimal HalfBalayagePrice { get; set; }

        [Display(Name="balayage cheveux courts"),DisplayFormat(DataFormatString="{0:C}")]
        public decimal ShortBalayagePrice { get; set; }


        [Display(Name="Mise en plis cheveux longs"),DisplayFormat(DataFormatString="{0:C}")]

        public decimal LongFoldingPrice { get; set; }

        [Display(Name="Mise en plis cheveux mi-longs"),DisplayFormat(DataFormatString="{0:C}")]
        public decimal HalfFoldingPrice { get; set; }

        [Display(Name="Mise en plis cheveux courts"),DisplayFormat(DataFormatString="{0:C}")]
        public decimal ShortFoldingPrice { get; set; }

        [Display(Name="Shampoing"),DisplayFormat(DataFormatString="{0:C}")]

        public decimal ShampooPrice { get; set; }

        [Display(Name="Soin"),DisplayFormat(DataFormatString="{0:C}")]

        public decimal CarePrice { get; set; }

        [Display(Name="Remise au forfait coupe+technique"),DisplayFormat(DataFormatString="{0:C}")]

        public decimal FlatFeeDiscount { get; set; }

    }
}
