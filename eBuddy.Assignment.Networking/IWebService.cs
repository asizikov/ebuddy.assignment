using System;
using eBuddy.Assignment.Networking.Parsers;

namespace eBuddy.Assignment.Networking
{
    public interface IWebService
    {
        IObservable<EmfMessage> GetMessage(string serverMessage, string server, int port);
        IObservable<BalancerResponce> GetBalancerResponce(string balancerMessage);
    }
}