using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Billing;
using Yavsc.Models.Billing;
using Yavsc.Server.Models.IT.SourceCode;

namespace Yavsc.Server.Models.IT
{
    public class Project : NominativeServiceCommand
    {
        [Key]
        public override long Id { get; set ; }
        public string OwnerId { get; set; }
        
        public string LocalRepo { get; set; }
        
        public string Name { get; set; }
        public string Version { get; set; }

        public string[] Configurations { get; set; }
        
        [ForeignKey("LocalRepo")]
        public virtual GitRepositoryReference Repository { get; set; }

        List<IBillItem> bill = new List<IBillItem> ();
        public void AddBillItem(IBillItem item)
        {
            bill.Add(item);
            
        }
        public override List<IBillItem> GetBillItems()
        {
            return bill;
        }



        public string Description { get; set; }

        public override string GetDescription()
        {
            return Description;
        }



        public Project()
        {
            
        }
    }
}