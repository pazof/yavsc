using BookAStar.Data;
using BookAStar.Helpers;
using BookAStar.Model.Interfaces;
using Newtonsoft.Json;
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
        public IList<BillingLine> Bill { get; set; }
        /// <summary>
        /// List of attached graphic files
        /// to this estimate, as relative pathes to
        /// the command performer's root path.
        /// In db, they are separated by <c>:</c>
        /// </summary>
        /// <returns></returns>
        public IList<string> AttachedGraphics { get; set; }
        [JsonIgnore]
        public string AttachedGraphicsString
        {
            get { return AttachedGraphics==null?null:string.Join(":", AttachedGraphics); }
            set { AttachedGraphics = value.Split(':').ToList(); }
        }
        /// <summary>
        /// List of attached files
        /// to this estimate, as relative pathes to
        /// the command performer's root path.
        /// In db, they are separated by <c>:</c>
        /// </summary>
        /// <returns></returns>
        public IList<string> AttachedFiles { get; set; }
        [JsonIgnore]
        public string AttachedFilesString
        {
            get { return AttachedFiles == null ? null : string.Join(":", AttachedFiles); }
            set { AttachedFiles = value.Split(':').ToList(); }
        }
        
        public string OwnerId { get; set; }
        
        public string ClientId { get; set; }
        [JsonIgnore]
        public BookQueryData Query
        {
            get
            {
                if (CommandId.HasValue)
                {
                    var dm = DataManager.Current;
                    return dm.BookQueries.LocalGet(CommandId.Value);
                }
                return null;
            }
        }
        [JsonIgnore]
        public ClientProviderInfo Client
        {
            get
            {
                return DataManager.Current.Contacts.LocalGet(ClientId);
            }
        }

        [JsonIgnore]
        public decimal Total { get
            {
                return Bill?.Aggregate((decimal)0, (t, l) => t + l.Count * l.UnitaryCost) ?? (decimal)0;
            }
        }
        
    }
}
