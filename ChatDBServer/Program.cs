using ChatDBNet.Udp;
using ChatDBServer.Connections;
using ChatDBServer.Services;

namespace ChatDBServer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var ms = new UdpMessageSource(12345);
            var db = new ChatDBResource();
            Server server = new Server(ms, db);
            server.Run();
        }
    }
}
