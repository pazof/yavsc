


namespace BookAStar.Model.Workflow.Marketing
{
    public class Skill {
        public string Name { get; set; }

        
        public long Id { get; set; }

        /// <summary>
        /// indice de recherche de cette capacité
        /// rendu par le système.
        /// Valide entre 0 et 100,
        /// Il démarre à 0.
        /// </summary>
        public int Rate { get; set; }
    }

}