using System;

namespace Client.Exceptions
{
    [Serializable]
    public class HostNotFoundException : Exception
    {
        public HostNotFoundException(Exception innerException) : base("Host not found", innerException) { }
    }
}
