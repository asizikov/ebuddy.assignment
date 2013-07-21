namespace eBuddy.Assignment.Networking
{
    public sealed class BalancerResponce
    {
        public BalancerResponce(bool success, string server, int port)
        {
            Success = success;
            Server = server;
            Port = port;
        }
        public bool Success { get; private set; }
        public string Server { get; private set;}
        public int Port { get; private set; }

        public override string ToString()
        {
            return string.Format("BalancerResponce: State = {0}; Server = {1}; Port = {2};", Success, Server, Port);
        }
    }
}