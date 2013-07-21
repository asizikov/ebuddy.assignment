using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace eBuddy.Assignment.Networking.Parsers
{
    public class EmfMessage
    {
        private readonly int _type;

        public EmfMessage(int type, long timespan, string account, string network, Dictionary<string,string> parameters)
        {
            if(timespan <= 0) throw new ArgumentOutOfRangeException("timespan");

            _type = type;
            TimeStamp = TimeSpan.FromMilliseconds(timespan);
            Account = account;
            Network = network;

            Parameters = parameters ?? new Dictionary<string, string>();
        }


        public MessageType Type { get { return (MessageType) _type; }}

        public TimeSpan TimeStamp { get; private set; }

        [CanBeNull]
        public string Account { get; private set; }

        [CanBeNull]
        public string Network { get; private set; }

        [NotNull]
        public Dictionary<string, string> Parameters { get; private set; }
    }
}