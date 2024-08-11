using ChatDB.Abstracts;
using ChatDB.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatDB.Services
{
    internal class UdpMessageSource : IMessageSource
    {
        private readonly UdpClient _udpClient;
        public UdpMessageSource()
        {
            _udpClient = new UdpClient();
        }
        public UdpMessageSource(int listeningPort)
        {
            _udpClient = new UdpClient(listeningPort);
        }
        public async Task<(NetMessage?, IPEndPoint)> ReceiveAsync()
        {
            var result = await _udpClient.ReceiveAsync();
            byte[] data = result.Buffer;
            var ep = result.RemoteEndPoint;
            string str = Encoding.UTF8.GetString(data);
            return (NetMessage.DeserializeFromJson(str), ep);
        }

        public async Task SendAsync(NetMessage message, IPEndPoint ep)
        {
            byte[] buffer = Encoding.UTF8.GetBytes
                (message.SerializeToJson());
            await _udpClient.SendAsync(buffer, buffer.Length, ep);
        }
    }
}
