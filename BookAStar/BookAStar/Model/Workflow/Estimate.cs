using BookAStar.Helpers;
using BookAStar.Model.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace BookAStar.Model.Workflow
{
    public partial class Estimate : IEstimate
    {
        public long Id { get; set; }
        public long? CommandId { get; set; }
        public string CommandType { get; set; }
        // Markdown expected
        public string Description { get; set; }
        public int? Status { get; set; }
        public string Title { get; set; }
        public List<BillingLine> Bill { get; set; }
        /// <summary>
        /// List of attached graphic files
        /// to this estimate, as relative pathes to
        /// the command performer's root path.
        /// In db, they are separated by <c>:</c>
        /// </summary>
        /// <returns></returns>
        public List<string> AttachedGraphicList { get; set; }

        public string AttachedGraphicsString
        {
            get { return AttachedGraphicList==null?null:string.Join(":", AttachedGraphicList); }
            set { AttachedGraphicList = value.Split(':').ToList(); }
        }
        /// <summary>
        /// List of attached files
        /// to this estimate, as relative pathes to
        /// the command performer's root path.
        /// In db, they are separated by <c>:</c>
        /// </summary>
        /// <returns></returns>
        public List<string> AttachedFiles { get; set; }
        public string AttachedFilesString
        {
            get { return AttachedFiles == null ? null : string.Join(":", AttachedFiles); }
            set { AttachedFiles = value.Split(':').ToList(); }
        }
        
        public string OwnerId { get; set; }
        
        public string ClientId { get; set; }
        
        public BookQueryData Query
        {
            get
            {
                if (CommandId.HasValue)
                {
                    return DataManager.Current.BookQueries.LocalGet(CommandId.Value);
                }
                return null;
            }
        }

        public ClientProviderInfo Client
        {
            get
            {
                return DataManager.Current.Contacts.LocalGet(ClientId);
            }
        }

        
        public decimal Total { get
            {
                return Bill?.Aggregate((decimal)0, (t, l) => t + l.Count * l.UnitaryCost) ?? (decimal)0;
            }
        }
        
    }
}
