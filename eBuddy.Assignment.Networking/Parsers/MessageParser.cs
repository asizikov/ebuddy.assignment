using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using JetBrains.Annotations;

namespace eBuddy.Assignment.Networking.Parsers
{
    internal sealed class MessageParser
    {
        private const int NUM = 5;
        private const char SEPARATOR = ':';
        private const char PARAMETERS_SEPARATOR = ';';


        [NotNull]
        public EmfMessage ParseMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ParseException("Message is null or empty.",message);

            var lines = message.Split(SEPARATOR).ToList();

            if(lines.Count != NUM) throw new ParseException("There must be " + NUM + "components!", message);

            int type;
            if (!Int32.TryParse(lines[0], out type))
            {
                throw new ParseException("an error in type", message);
            }

            long timestamp;
            if (!Int64.TryParse(lines[1], out timestamp))
            {
               throw new ParseException("an error in timestamp", message);
            }

            var account = lines[2];
            var network = lines[3];

            var parameters = lines[4];

            var dict = parameters.Split(PARAMETERS_SEPARATOR)
                                 .Select(line => line.Split('='))
                                 .Where(l => l.Count() == 2)
                                 .Select(pair =>
                                     {
                                         var decoded = HttpUtility.UrlDecode(pair[1]);
                                         return  new KeyValuePair<string, string>(pair[0],decoded);
                                     })
                                 .ToDictionary(keyValuePair => keyValuePair.Key, keyValuePair => keyValuePair.Value);
            return new EmfMessage(type,timestamp, account, network, dict);
        }
    }
}
