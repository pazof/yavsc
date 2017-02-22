using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XLabs.Forms.Mvvm;
using ZicMoove.Model.Social;
using ZicMoove.Model.Workflow.Messaging;
using ZicMoove.ViewModels.Validation;

namespace ZicMoove.ViewModels.WorkFlow
{
    class WorkflowBookViewModel : EditingViewModel<BookQuery>
    {
        public WorkflowBookViewModel(BookQuery data) : base(data)
        {
        }

        public override void Check()
        {
            throw new NotImplementedException();
        }
    }
}
