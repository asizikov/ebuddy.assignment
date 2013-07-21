namespace eBuddy.Assignment.Networking
{
    internal sealed class ServerResponce
    {
        public ServerResponce(bool success, string bannerId, int timeout)
        {
            Success = success;
            BannerId = bannerId;
            Timeout = timeout;
        }
        public bool Success { get; private set; }
        public string BannerId { get; private set;}
        public int Timeout { get; private set; }

        public override string ToString()
        {
            return string.Format("ServerResponce: State = {0}; BannerId = {1}; Timeout = {2};", Success, BannerId, Timeout);
        }
    }
}