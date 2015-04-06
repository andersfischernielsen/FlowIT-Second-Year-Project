using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class InvalidWorkflowIdException : Exception
    {
        public InvalidWorkflowIdException(Exception innerException) : base("WorkflowId does not exist", innerException) { }
    }

    public class ServerNotFoundException : Exception
    {
        public ServerNotFoundException(Exception innerException) : base("Server not found", innerException) { }
    }

    public class LoginFailedException : Exception
    {
        public LoginFailedException(Exception innerException) : base("Username or password didn't match", innerException) { }
    }
}
