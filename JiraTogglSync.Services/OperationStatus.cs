using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraTogglSync.Services
{
    public class OperationResult
    {
        public Status Status { get; protected set; }
        public string Message { get; protected set; }
        public WorkLogEntry OperationArgument { get; protected set; }

        protected OperationResult()
        {
        }

        public static OperationResult Success(WorkLogEntry arg)
        {
            return new OperationResult()
            {
                Status = Status.Success,
                OperationArgument = arg
            };
        }

        public static OperationResult Error(string message, WorkLogEntry arg)
        {
            return new OperationResult()
            {
                Status = Status.Error,
                Message = message,
                OperationArgument = arg
            };
        }
    }

    public enum Status
    {
        Success,
        Error
    }
}
