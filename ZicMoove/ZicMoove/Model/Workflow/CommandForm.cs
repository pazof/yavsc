using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yavsc;

namespace ZicMoove.Model.Workflow
{
    public class CommandForm : ICommandForm
    {
        public string ActionName
        {
            get; set;
        }

        public string ActivityCode
        {
            get; set;
        }

        public long Id
        {
            get; set;
        }

        public string Title
        {
            get; set;
        }
    }
}
