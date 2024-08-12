using System.Collections.Concurrent;
using System.Net;
using ChatDBNet.Message;
using ChatDBNet.Interfaces;
using ChatDBServer.Interfaces;

namespace ChatDBServer.Services
{
    public class Server
    {
        private readonly Dictionary<string, IPEndPoint> _onLineUsers = [];
        public HashSet<string> OnLineUsers { get => [.. _onLineUsers.Keys]; }
        private readonly IMessageSource _messageSource;
        private readonly IDBResource _db;
        private readonly CancellationTokenSource _serverStopTokenSource = new();
        private readonly ConcurrentDictionary<int, KeepUser> _notConfirmed = [];
        public Server(IMessageSource messageSource, IDBResource db)
        {
            this._messageSource = messageSource;
            this._db = db;
        }
        public void Run()
        {
            Console.WriteLine("Сервер запущен.");
            _messageSource.StartReceivingAsync();
            Task.WaitAll([Task.Run(CancellationRequest), Task.Run(WorkingCycle)]);
        }
        private void CancellationRequest()
        {
            Console.WriteLine("Нажмите любую клавишу для того, чтобы завершить работу.");
            //Console.ReadKey();
            // TODO: Вернуть к ReadKey()
            Thread.Sleep(new TimeSpan(1, 0, 0));
            Console.WriteLine();
            Console.WriteLine("Работа сервера будет завершена после получения следующего сообщения.");
            _serverStopTokenSource.Cancel();
        }
        private void WorkingCycle()
        {
            while (true && !_serverStopTokenSource.IsCancellationRequested)
            {
                if (_messageSource.InBox.Count > 0)
                {
                    var tuple = _messageSource.InBox.Take();
                    HandleMessage(tuple.Item1, tuple.Item2);
                }
            }
            _messageSource.StopReceiving();
        }
        private async Task SendConfirmationAsync(NetMessage message, IPEndPoint ip)
        {
            await _messageSource.SendAsync(NetMessage.CreateConfirmation(message), ip);
        }
        private async Task HandleMessage(NetMessage message, IPEndPoint ip)
        {
            Console.WriteLine($"С IP адреса {ip.Address}:{ip.Port} получено сообщение:");
            Console.WriteLine($"\t{message}");

            switch (message.MessageType)
            {
                case MessageType.SIMPLE: await HandleSimpleMessage(message); break;
                case MessageType.LIST: break;
                case MessageType.REGISTRATION: await RegisterClientAsync(message); break;
                case MessageType.AUTHENTIFICATION: throw new NotImplementedException();
                case MessageType.CONFIRMATION: RemoveFromConfirmationDictionary(message); break;
                default: Console.WriteLine("Неизвестный тип сообщений"); break;
            }
        }
        private async Task RegisterClientAsync(NetMessage message)
        {
            if (IPEndPoint.TryParse(message.Text, out var ip))
            {
                await _messageSource.SendAsync(NetMessage.CreateConfirmation(message), ip);

                string nick = message.UserFrom;
                if (nick.ToLower().Equals("server") || nick.ToLower().Equals("public") || nick.ToLower().Equals("admin"))
                {
                    await _messageSource.SendAsync(new() { Text = $"Имя пользователя {nick} недопустимо. Повторите регистрацию." }, ip);
                }
                else
                {
                    _onLineUsers.TryAdd(nick, ip);
                    if (_db.GetUserID(nick) == -1)
                    {
                        _db.AddUser(nick, out int ID);
                        NetMessage msg = new() { Text = $"Успешная регистрация! Ваш ID: {ID}." };
                        await SendMessage(msg, nick);
                    }
                    else
                    {
                        await _messageSource.SendAsync(new() { Text = "Пользователь с таким именем уже зарегистрирован." }, ip);
                    }
                }
            }
            else
            {
                Console.WriteLine("Ошибка парсинга клиентского EndPoint.");
            }
        }
        private async Task<bool> SendMessage(NetMessage msg, string nick)
        {
            AddToConfirmationDictionary(msg.GetHashCode(), nick, out var ct);
            await _messageSource.SendAsync(msg, _onLineUsers[nick]);
            DeleteIfNotConfirmed(msg.GetHashCode(), nick, ct, out var deleted);
            return !deleted;
        }

        private void AddToConfirmationDictionary(int messageHash, string nick, out CancellationToken cancellationToken)
        {
            var cts = new CancellationTokenSource();
            void cansel()
            {
                cts.Cancel();
            }
            KeepUser keepUser;
            keepUser = cansel;
            _notConfirmed.TryAdd(messageHash, keepUser);
            cancellationToken = cts.Token;
        }

        private delegate void KeepUser();

        private void DeleteIfNotConfirmed(int messageHash, string nick, CancellationToken cancellationToken, out bool deleted)
        {
            Thread.Sleep(10_000);
            if (!cancellationToken.IsCancellationRequested)
            {
                if (_onLineUsers.ContainsKey(nick))
                {
                    _onLineUsers.Remove(nick);
                    _notConfirmed.TryRemove(messageHash, out _);
                }
                deleted = true;
            }
            else deleted = false;
        }

        private void RemoveFromConfirmationDictionary(NetMessage message)
        {
            if (_notConfirmed.TryGetValue(message.GetHashCode(), out var keepUser))
            {
                keepUser();
                _notConfirmed.TryRemove(message.GetHashCode(), out _);
            }
        }

        private async Task HandleSimpleMessage(NetMessage msg)
        {
            if (_onLineUsers.ContainsKey(msg.UserFrom))
            {
                _messageSource.SendAsync(NetMessage.CreateConfirmation(msg), _onLineUsers[msg.UserFrom]);
                if (msg.UserTo.ToLower().Equals("public"))
                {
                    _db.AddMessage(msg, true, out _);
                    foreach (var user in _onLineUsers)
                    {
                        SendMessage(msg, user.Key);
                    }
                }
                else
                {
                    if (_onLineUsers.ContainsKey(msg.UserTo))
                    {
                        var sending = SendMessage(msg, msg.UserTo);
                        sending.Wait();
                        _db.AddMessage(msg, sending.Result, out _);
                    }
                    else
                    {
                        if (_db.GetUserID(msg.UserTo) >= 0)
                        {
                            _db.AddMessage(msg, false, out _);
                        }
                        else
                        {
                            await SendMessage(new NetMessage() { Text = "Получатель не зарегистрирован." }, msg.UserFrom);
                        }
                    }
                }
            }
        }
    }
}
