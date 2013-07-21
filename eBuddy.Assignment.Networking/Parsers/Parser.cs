using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace eBuddy.Assignment.Networking.Parsers
{
    internal sealed class Parser
    {
        public BalancerResponce ParceLoaderResponce(string message)
        {
            try
            {
                var result = FindVal(message, MessageKeys.Result);
                var server = FindVal(message, MessageKeys.Server);
                var port = FindVal(message, MessageKeys.Port);
                var responce = new BalancerResponce(result == "OK", server, Int32.Parse(port));
                return responce;
            }
            catch (Exception)
            {
                return new BalancerResponce(false, string.Empty, default(int));
            }
            
        }

        public ServerResponce ParceBannerInfo(string message)
        {
            try
            {
                var result = FindVal(message, MessageKeys.Result);
                var bannerId = FindVal(message, MessageKeys.BannerId);
                var timeout = FindVal(message, MessageKeys.TimeOut);
                var responce = new ServerResponce(result == "OK", bannerId, Int32.Parse(timeout));
                return responce;
            }
            catch (Exception)
            {
                return new ServerResponce(false, string.Empty, default(int));
            }
        }

        private static string FindVal(string str, string paramName)
        {
            var pattern = new Regex(string.Format(@"{0}=(?<value>[A-z.\-0-9]+);", paramName),
                RegexOptions.Compiled | RegexOptions.Singleline);
            foreach (var match in pattern.Matches(str)
                                         .Cast<Match>()
                                         .Where(match => match.Success))
            {
                return match.Groups["value"].Value;
            }
            return string.Empty;
        }
    }
}
