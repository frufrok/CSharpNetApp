using ChatDBNet.Interfaces;
using ChatDBNet.Message;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChatDBClient
{
    public class Client
    {
        public string Name { get; init; }
        public IPEndPoint? ServerEndPoint { get; init; }
        private readonly IMessageSource _messageSource;
        private bool _registered;
        public Client(string userName, IPEndPoint serverEndPoint, IMessageSource messageSource)
        {
            Name = userName;
            ServerEndPoint = serverEndPoint;
            _messageSource = messageSource;
            Console.WriteLine($"Запущен клиент {this.Name}.");
            Console.WriteLine("Адрес сервера:");
            Console.WriteLine($"{ServerEndPoint?.Address.ToString()}:{ServerEndPoint?.Port}");
        }
        public void Run()
        {
            Console.WriteLine("Клиент запущен.");
            _messageSource.StartReceivingAsync();
            Console.WriteLine("Запущено прослушивание входящих сообщений.");
            Task.Run(this.PrintInbox);
            Console.WriteLine("Запущен вывод входящих сообщений в консоль.");
            if (RegisterClient()) StartWorkingCycle();
            else Console.WriteLine("Регистрация клиента не удалась. Работа программы завершена.");
        }
        private bool RegisterClient()
        {
            Console.WriteLine("Регистрация клиента");
            if (ServerEndPoint != null)
            {
                Task.Run(() => _messageSource.SendAsync(NetMessage.CreateRegistration(Name, new IPEndPoint(IPAddress.Any, 0)), ServerEndPoint));
                
                for (int i =0; i < 10 && !_registered; i++)
                {
                    Console.WriteLine(".");
                    Thread.Sleep(1000);
                }
                return _registered;
            }
            else
            {
                throw new Exception("Server IPEndPoint is null. Cannot register client.");
            }
        }
        private async Task SendMessageAsync(string to, string text)
        {
            if (ServerEndPoint != null)
            {
                var msg = new NetMessage()
                {
                    UserFrom = this.Name,
                    UserTo = to,
                    Text = text
                };
                await _messageSource.SendAsync(msg, ServerEndPoint);
            }
            else
            {
                Console.WriteLine("EndPoint сервера недоступен.");
            }
        }
        private void PrintInbox()
        {
            while (true)
            {
                if (_messageSource.InBox.Count > 0)
                {
                    var msg = _messageSource.InBox.Take().Item1;

                    if (!_registered)
                    {
                        if (msg.UserFrom.ToLower().Equals("server") && 
                            (
                                msg.Text.ToLower().Contains("Успешная регистрация!".ToLower())
                                ||msg.Text.ToLower().Equals("Вы уже зарегистрированы.".ToLower())
                            ))
                        {
                            Console.WriteLine("Регистрация успешна.");
                            _registered = true;
                        }
                    }
                    switch (msg.MessageType)
                    {
                        case MessageType.SIMPLE: Console.WriteLine(msg); break;
                        case MessageType.LIST: NetMessage.ExtractMessages(msg)?.ForEach(x => Console.WriteLine(x)); break;
                        case MessageType.CONFIRMATION: Console.WriteLine("Сообщение доставлено."); break;
                    }
                }
            }
        }
        private void StartWorkingCycle()
        {
            while (true)
            {
                while (true)
                {
                    string? text;
                    string? toName;
                    bool exitFlag;
                    do
                    {
                        Console.WriteLine("Введите адресата сообщения или введите 'public', чтобы написать в общий чат:");
                        toName = Console.ReadLine();
                        exitFlag = toName != null && toName.ToLower().Equals("exit");
                    }
                    while (string.IsNullOrEmpty(toName) && !exitFlag);
                    do
                    {
                        Console.WriteLine("Введите сообщение:");
                        text = Console.ReadLine();
                        exitFlag = text != null && text.ToLower().Equals("exit");
                    }
                    while (string.IsNullOrEmpty(text) && !exitFlag);
                    if (exitFlag)
                    {
                        Console.WriteLine("Работа клиента завершена.");
                        break;
                    }
                    else Task.Run(() => SendMessageAsync(toName ?? String.Empty, text ?? String.Empty));
                }
            }
        }
    }
}
