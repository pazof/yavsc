using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models.Haircut
{
    public class HairPrestation
    { 
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set;} 

        public HairLength Length { get; set; } 
        public HairCutGenders Gender { get; set; } 
        public bool Cut { get; set; } 

        public HairDressings Dressing { get; set; } 
        public HairTechnos Tech { get; set; } 
        public bool Shampoo { get; set; } 
        public HairTaint[] Taints { get; set; } 
        public  bool Cares { get; set; } 
        
    }
}