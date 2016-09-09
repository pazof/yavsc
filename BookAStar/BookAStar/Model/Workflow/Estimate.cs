using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookAStar.Model.Workflow
{
    public partial class Estimate
    {
        public long Id { get; set; }

        public long? CommandId { get; set; }
        /// <summary>
        /// A command is not required to create
        /// an estimate,
        /// it will result in a new estimate template
        /// </summary>
        /// <returns></returns>
        public BookQueryPage Query { get; set; }
        public string Description { get; set; }
        public int? Status { get; set; }
        public string Title { get; set; }
        public List<CommandLine> Bill { get; set; }
        /// <summary>
        /// List of attached graphic files
        /// to this estimate, as relative pathes to
        /// the command performer's root path.
        /// In db, they are separated by <c>:</c>
        /// </summary>
        /// <returns></returns>
        public List<string> AttachedGraphicList { get; private set; }

        public string AttachedGraphicsString
        {
            get { return string.Join(":", AttachedGraphicList); }
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
            get { return string.Join(":", AttachedFiles); }
            set { AttachedFiles = value.Split(':').ToList(); }
        }
        
        public string OwnerId { get; set; }
        private RemoteEntity<BookQueryData, long> BookQueries;
        
        public string ClientId { get; set; }
    }
}
