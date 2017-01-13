

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models {
    public class Skill {
        public string Name { get; set; }

        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// indice de recherche de ce talent
        /// rendu par le système.
        /// Valide entre 0 et 100,
        /// Il démarre à 0.
        /// </summary>
        public int Rate { get; set; }
    }

}