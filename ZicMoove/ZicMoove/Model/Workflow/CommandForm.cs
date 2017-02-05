using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YavscLib;

namespace ZicMoove.Model.Workflow
{
    public class CommandForm : ICommandForm
    {
        public string Action
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
