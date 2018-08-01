using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Yavsc.Abstract.IT;
using Yavsc.Billing;
using Yavsc.Models.Billing;
using Yavsc.Server.Models.IT.SourceCode;

namespace Yavsc.Server.Models.IT
{
    public class Project : NominativeServiceCommand, IProject
    {
        [Key]
        public override long Id { get; set; }
        public string OwnerId { get; set; }

        /// <summary>
        /// This field is something like a key,
        /// since it is required non null,
        /// and is the key of the foreign GitRepositoryReference entity.
        /// 
        /// As a side effect, there's no project without valid git reference in db. 
        /// </summary>
        /// <returns></returns>
        [Required]
        public string Name { get; set; }

        public string Version { get; set; }

        [InverseProperty("TargetProject")]
        public virtual List<ProjectBuildConfiguration> Configurations { get; set; }

        [ForeignKey("Name")]
        public virtual GitRepositoryReference Repository { get; set; }

        List<IBillItem> bill = new List<IBillItem>();
        public void AddBillItem(IBillItem item)
        {
            bill.Add(item);

        }
        public override List<IBillItem> GetBillItems()
        {
            return bill;
        }

        public IEnumerable<string> GetConfigurations()
        {
            return Configurations.Select(c => c.Name);
        }

        string description;
        public override string Description 
        { 
            get { return description; } 
            set { description = value; }
        }

        public Project()
        {

        }
    }
}


