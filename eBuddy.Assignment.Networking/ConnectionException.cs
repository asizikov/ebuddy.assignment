using System;

namespace eBuddy.Assignment.Networking
{
    public class ConnectionException : Exception
    {
        private readonly string _message;

        public ConnectionException(string message)
        {
            _message = message;
        }

        public override string Message
        {
            get
            {
                return _message;
            }
        }
    }
}