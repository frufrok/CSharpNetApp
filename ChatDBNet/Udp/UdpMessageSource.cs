using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ChatDBNet.Message;
using ChatDBNet.Interfaces;
using System.Collections.Concurrent;
using System.Net.Sockets;


namespace ChatDBNet.Udp
{
    public class UdpMessageSource : IMessageSource
    {
        private readonly CancellationTokenSource _stopReceivingTokenSource = new CancellationTokenSource();
        public int ListeningPort { get; init; }
        public IPAddress LocalAddress { get; init; }
        public UdpClient ListeningUdpClient { get; init; }
        public UdpClient SendingUdpClient { get; init; }
        public BlockingCollection<(NetMessage, IPEndPoint)> InBox { get; init; } = [];
        
        public UdpMessageSource(ushort listeningPort)
        {
            this.LocalAddress = GetLocalIPAddress();
            this.SendingUdpClient = new UdpClient();
            this.ListeningUdpClient = listeningPort > 0 ? new UdpClient(listeningPort) : new UdpClient();
            ArgumentNullException.ThrowIfNull(this.ListeningUdpClient.Client.LocalEndPoint);
            this.ListeningPort = ((IPEndPoint)this.ListeningUdpClient.Client.LocalEndPoint).Port;
        }

        public async Task SendAsync(NetMessage message, IPEndPoint receiver)
        {
            string json = message.SerializeToJson();
            var data = Encoding.UTF8.GetBytes(json);
            await SendingUdpClient.SendAsync(data, data.Length, receiver);
        }

        public async Task StartReceivingAsync(Action<NetMessage, IPEndPoint> preliminaryHandling)
        {
            var stop = _stopReceivingTokenSource.Token;
            while (!stop.IsCancellationRequested)
            {
                var result = await ListeningUdpClient.ReceiveAsync();
                var messageJson = Encoding.UTF8.GetString(result.Buffer);
                NetMessage? message = NetMessage.DeserializeFromJson(messageJson);
                if (message != null)
                {
                    preliminaryHandling(message, result.RemoteEndPoint);
                    InBox.Add((message, result.RemoteEndPoint));
                }
            }
        }

        public void StopReceiving()
        {
            this._stopReceivingTokenSource.Cancel();
        }

        public static IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork) return ip;
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
