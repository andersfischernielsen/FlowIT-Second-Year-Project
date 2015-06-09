using System;

namespace Client.Exceptions
{
    [Serializable]
    public class LoginFailedException : Exception
    {
        public LoginFailedException(Exception innerException) : base("Username or password didn't match", innerException) { }
    }
}