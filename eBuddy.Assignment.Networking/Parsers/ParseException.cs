using System;

namespace eBuddy.Assignment.Networking.Parsers
{
    public class ParseException: Exception
    {
        private readonly string _message;
        private readonly string _badInput;

        public ParseException(string message, string badInput = null)
        {
            _message = message;
            _badInput = badInput;
        }

        public override string Message
        {
            get { return _message; }
        }

        public string BadInput
        {
            get { return _badInput; }
        }
    }
}