#nullable enable annotations

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Newtonsoft.Json;

namespace Yavsc.Models.Haircut
{
    public class HairPrestation
    {
        // Homme ou enfant => Coupe seule
        // Couleur => Shampoing
        // Forfaits : Coupe + Technique
        // pas de coupe => technique

        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Display(Name="Longueur de cheveux")]
        public HairLength Length { get; set; }

        [Display(Name="Pour qui")]
        public HairCutGenders Gender { get; set; }

        [Display(Name="Coupe")]
        public bool Cut { get; set; }

        [Display(Name="Coiffage")]

        public HairDressings Dressing { get; set; }

        [Display(Name="Technique")]
        public HairTechnos Tech { get; set; }

        [Display(Name="Shampoing")]
        public bool Shampoo { get; set; }

        [Display(Name="Couleurs"),JsonIgnore,InverseProperty("Prestation")]

        public virtual List<HairTaintInstance> Taints { get; set; }

        [Display(Name="Soins")]
        public  bool Cares { get; set; }

        public string GetDisplayTitle()
        {
            return $"{GetEnumDisplayName(Gender)} · {GetEnumDisplayName(Length)}";
        }

        public string GetDisplayDetails()
        {
            return string.Join(" · ", new[]
            {
                FormatFlag(nameof(Cut), Cut),
                GetEnumDisplayName(Dressing),
                GetEnumDisplayName(Tech),
                FormatFlag(nameof(Shampoo), Shampoo),
                FormatFlag(nameof(Cares), Cares),
            });
        }

        private static string FormatFlag(string propertyName, bool enabled)
        {
            var label = GetPropertyDisplayName(propertyName);
            return enabled ? label : $"Sans {label.ToLowerInvariant()}";
        }

        private static string GetPropertyDisplayName(string propertyName)
        {
            var property = typeof(HairPrestation).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            return property?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? propertyName;
        }

        private static string GetEnumDisplayName<TEnum>(TEnum value) where TEnum : struct, Enum
        {
            var member = typeof(TEnum).GetMember(value.ToString()).FirstOrDefault();
            return member?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? value.ToString();
        }


    }
    public class HairTaintInstance {

        public long TaintId { get; set; }

        [ForeignKey("TaintId")]
        public virtual HairTaint Taint { get; set; }
        public long PrestationId { get; set; }

        [ForeignKey("PrestationId")]
        public virtual HairPrestation Prestation { get; set; }
    }
}
