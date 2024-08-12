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
    public class UdpMessageSource : IMessageSource, IDisposable
    {
        private readonly CancellationTokenSource _stopReceivingTokenSource = new CancellationTokenSource();
        public ushort ListeningPort { get; init; }
        public IPAddress LocalAddress { get; init; }
        public IPEndPoint ListeningEndPoint { get => new(LocalAddress, ListeningPort); }
        public UdpClient ListeningUdpClient { get; init; }
        public UdpClient SendingUdpClient { get; init; }
        public BlockingCollection<(NetMessage, IPEndPoint)> InBox { get; init; } = [];
        
        public UdpMessageSource(ushort listeningPort = 0)
        {
            this.LocalAddress = GetLocalIPAddress();
            var localEP = new IPEndPoint(this.LocalAddress, listeningPort);
            this.ListeningPort = (ushort)localEP.Port;
            this.ListeningUdpClient = new UdpClient();
            this.ListeningUdpClient.Client.Bind(localEP);
            this.SendingUdpClient = new UdpClient();
        }

        public async Task SendAsync(NetMessage message, IPEndPoint receiver)
        {
            if (message.MessageType == MessageType.REGISTRATION)
            {
                message.Text = $"{this.LocalAddress}:{this.ListeningPort}";
            }
            string json = message.SerializeToJson();
            var data = Encoding.UTF8.GetBytes(json);
            await SendingUdpClient.SendAsync(data, data.Length, receiver);
        }

        public async Task StartReceivingAsync()
        {
            await StartReceivingAsync((x, y) => { });
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

        public void Dispose()
        {
            this.ListeningUdpClient.Dispose();
            this.SendingUdpClient.Dispose();
        }
    }
}
