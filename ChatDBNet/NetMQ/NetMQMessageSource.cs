using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ChatDBNet.Interfaces;
using ChatDBNet.Message;
using NetMQ;
using NetMQ.Sockets;

namespace ChatDBNet.NetMQ
{
    public class NetMQMessageSource : IMessageSource, IDisposable
    {
        private readonly CancellationTokenSource _stopReceivingTokenSource = new CancellationTokenSource();
        public ushort ListeningPort { get; init; }
        public IPAddress LocalAddress { get; init; }
        public IPEndPoint ListeningEndPoint { get => new(LocalAddress, ListeningPort); }
        public ResponseSocket ListeningSocket { get; init; }
        public RequestSocket SendingSocket { get; init; }
        public BlockingCollection<(NetMessage, IPEndPoint)> InBox { get; init; } = [];

        public NetMQMessageSource(ushort listeningPort = 0)
        {
            this.LocalAddress = GetLocalIPAddress();
            var localEP = new IPEndPoint(this.LocalAddress, listeningPort);
            this.ListeningPort = (ushort)localEP.Port;

            this.ListeningSocket = new(GetConnectionString(this.ListeningEndPoint));
            this.SendingSocket = new();
        }

        public async Task SendAsync(NetMessage message, IPEndPoint receiver)
        {
            if (message.MessageType == MessageType.REGISTRATION)
            {
                message.Text = $"{this.LocalAddress}:{this.ListeningPort}";
            }
            string json = message.SerializeToJson();
            await Task.Run(() =>
            {
                this.SendingSocket.Connect(GetConnectionString(receiver));
                this.SendingSocket.SendFrame(json);
            });
        }

        public async Task StartReceivingAsync()
        {
            await StartReceivingAsync((x, y) => { });
        }

        public async Task StartReceivingAsync(Action<NetMessage, IPEndPoint> preliminaryHandling)
        {
            await Task.Run(() =>
            {
                var stop = _stopReceivingTokenSource.Token;
                while (!stop.IsCancellationRequested)
                {
                    var result = this.ListeningSocket.ReceiveFrameString();
                    var messageJson = result;
                    NetMessage? message = NetMessage.DeserializeFromJson(messageJson);
                    if (message != null)
                    {
                        var ip = new IPEndPoint(IPAddress.Any, 0);
                        preliminaryHandling(message, ip);
                        InBox.Add((message, ip));
                    }
                }
            });
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

        public static string GetConnectionString(IPEndPoint ipEndPoint)
        {
            return $"tcp://{ipEndPoint.Address}:{ipEndPoint.Port}";
        }

        public void Dispose()
        {
            this.ListeningSocket.Dispose();
            this.SendingSocket.Dispose();
        }
    }
}
