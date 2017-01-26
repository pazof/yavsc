

namespace BookAStar.Model.Workflow.Marketing
{
    public partial class UserSkills
    {
        public long Id { get; set; }

        public string UserId { get; set; }
        
        public long SkillId { get; set; }

        public string Comment { get; set; }
        public int Rate { get; set; }
    }
}
