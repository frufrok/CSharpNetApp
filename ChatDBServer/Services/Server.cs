using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatDBNet.Interfaces;
using ChatDBNet.Message;
using ChatDB.Models;
using System.Collections.Concurrent;
using System.Net;

namespace ChatDBServer.Services
{
    public class Server
    {
        private readonly IMessageSource _messageSource;
        public Server(int listeningPort = 0)
        {
            _messageSource = listeningPort > 0 ? new UdpMessageSource(listeningPort) : new UdpMessageSource();
        }
        private ConcurrentDictionary<string, IPEndPoint> _clients = [];

        public async void Start()
        {
            Console.WriteLine("Сервер ожидает сообщения.");
            while (true)
            {
                try
                {
                    var result = await _messageSource.ReceiveAsync();
                    Console.WriteLine(result.Item1);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private async Task ProcessMessageAsync(NetMessage message, IPEndPoint fromIP)
        {
            switch (message.MessageType)
            {
                case MessageType.SIMPLE:
                    break;
                case MessageType.LIST:
                    break;
                case MessageType.REGISTRATION:
                    await RegisterUserAsync(message, fromIP);
                    break;
                case MessageType.CONFIRMATION:
                    break;
            }
        }

        private async Task RegisterUserAsync(NetMessage message, IPEndPoint fromIP)
        {
            Console.WriteLine($"Регистрация клиента {message.UserFrom}");
            await Task.Run(() =>
            {
                if (_clients.TryAdd(message.UserFrom, fromIP))
                {
                        using var context = new ChatDBContext();
                        context.Users.Add(new User() { Nickname = message.UserFrom });
                        context.SaveChanges();
                }
            });
        }
        private async Task RelyMessage(NetMessage message)
        {
            await Task.Run(() =>
            {
                throw new NotImplementedException();
                /*if (_clients.ContainsKey(message.UserFrom))
                {

                }
                */
            });
        }

    }
}
