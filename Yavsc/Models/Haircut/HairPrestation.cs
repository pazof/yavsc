
using Yavsc.Models.Market;

namespace Yavsc.Models.Haircut
{
    public class HairPrestation : Service
    { 
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