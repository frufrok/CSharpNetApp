using System.Collections.Concurrent;
using System.Net;
using ChatDBNet.Message;

namespace ChatDBNet.Interfaces
{
    public interface IMessageSource
    {
        Task SendAsync(NetMessage message, IPEndPoint receiver);
        Task StartReceivingAsync();
        void StopReceiving();
        BlockingCollection<(NetMessage, IPEndPoint)> InBox { get; init; }
    }
}
