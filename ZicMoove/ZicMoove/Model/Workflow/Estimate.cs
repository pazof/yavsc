
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZicMoove.Model.Workflow
{
    using Data;
    using Musical;
    using YavscLib.Workflow;

    public partial class Estimate : IEstimate
    {
        public long Id { get; set; }
        public long? CommandId { get; set; }
        public string CommandType { get; set; }
        // Markdown expected
        public string Description { get; set; }
        public string Title { get; set; }
        public List<BillingLine> Bill { get; set; }
        /// <summary>
        /// List of attached graphic files
        /// to this estimate, as relative pathes to
        /// the command performer's root path.
        /// In db, they are separated by <c>:</c>
        /// </summary>
        /// <returns></returns>
        public List<string> AttachedGraphics { get; set; }
        [JsonIgnore]
        // form in db
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
        public List<string> AttachedFiles { get; set; }
        // form in db
        [JsonIgnore]
        public string AttachedFilesString
        {
            get { return AttachedFiles == null ? null : string.Join(":", AttachedFiles); }
            set { AttachedFiles = value.Split(':').ToList(); }
        }
        
        public string OwnerId { get; set; }
        [JsonIgnore]
        public ClientProviderInfo Owner
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(OwnerId))
                {
                    var dm = DataManager.Instance;
                    return dm.Contacts.LocalGet(OwnerId);
                }
                return null;
            }
        }
        public string ClientId { get; set; }
        [JsonIgnore]
        public BookQuery Query
        {
            get
            {
                if (CommandId.HasValue)
                {
                    var dm = DataManager.Instance;
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
                return DataManager.Instance.Contacts.LocalGet(ClientId);
            }
        }

        [JsonIgnore]
        public decimal Total { get
            {
                return Bill?.Aggregate((decimal)0, (t, l) => t + l.Count * l.UnitaryCost) ?? (decimal)0;
            }
        }
        /// <summary>
        /// This validation comes first from the provider.
        /// </summary>
        public DateTime ProviderValidationDate { get; set; }
        /// <summary>
        /// Date for the agreement from the client
        /// </summary>
        public DateTime ClientApprouvalDate { get; set; }

    }
}
