using ChatDBNet.NetMQ;
using ChatDBNet.Udp;
using ChatDBServer.Connections;
using ChatDBServer.Services;

namespace ChatDBServerMQ
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var ms = new NetMQMessageSource(12345);
            var db = new ChatDBResource();
            Server server = new Server(ms, db);
            server.Run();
        }
    }
}
