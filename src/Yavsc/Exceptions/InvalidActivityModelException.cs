using System;
namespace Yavsc.Exceptions
{
    public class InvalidWorkflowModelException : Exception
    {
        public InvalidWorkflowModelException(string descr) : base(descr)
        {

        }
        public InvalidWorkflowModelException(string descr, Exception inner) : base(descr,inner)
        {
            
        }
    }
}