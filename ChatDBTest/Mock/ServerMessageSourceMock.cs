using ChatDBNet.Interfaces;
using ChatDBNet.Message;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChatDBTest.Mock
{
    public class ServerMessageSourceMock : IMessageSource
    {
        public BlockingCollection<(NetMessage, IPEndPoint)> InBox { get; init; } = [];
        public List<(NetMessage, IPEndPoint)> OutBox { get; init; } = [];

        public async Task SendAsync(NetMessage message, IPEndPoint receiver)
        {
            OutBox.Add((message, receiver));
        }

        public async Task StartReceivingAsync()
        {

        }

        public void StopReceiving()
        {

        }
    }
}
