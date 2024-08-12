using ChatDBNet.Message;
using ChatDBNet.NetMQ;
using ChatDBNet.Udp;
using NetMQ;
using NetMQ.Sockets;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatDBTest
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class NetMQMessageSourceTest
    {
        [Test]
        public async Task SendingSocketIsNotNull()
        {
            static void Check()
            {
                var ms = new NetMQMessageSource();
                Assert.That(ms.SendingSocket, Is.Not.Null);
            }
            await Task.Run(Check);
        }

        [Test]
        public async Task ListeningSocketIsNotNull()
        {
            static void Check()
            {
                var ms = new NetMQMessageSource();
                Assert.That(ms.ListeningSocket, Is.Not.Null);
            }
            await Task.Run(Check);
        }

        [Test]
        public async Task ListeningSocketHasGivenPort()
        {
            static void Check()
            {
                ushort port;
                using (var tempMS = new NetMQMessageSource())
                {
                    port = tempMS.ListeningPort;
                }
                var ms = new NetMQMessageSource(port);
                Assert.That(ms.ListeningPort, Is.EqualTo(port));
            }
            await Task.Run(Check);
        }

        [Test]
        public async Task SendingTest()
        {
            static void Check()
            {
                ushort port = 12345;
                IPEndPoint localEP;
                ResponseSocket receiver;

                while(true)
                {
                    try
                    {
                        localEP = new IPEndPoint(NetMQMessageSource.GetLocalIPAddress(), port);
                        receiver = new ResponseSocket(NetMQMessageSource.GetConnectionString(localEP));
                        break;
                    }
                    catch
                    {
                        port++;
                    }
                }

                var ms = new NetMQMessageSource();
                
                var message = new NetMessage()
                {
                    UserFrom = "User1",
                    UserTo = "User2",
                    Text = "Hello!"
                };

                var receiving = Task.Run(() => receiver.ReceiveFrameString());
                var sending = ms.SendAsync(message, localEP);

                sending.Wait(1000);
                receiving.Wait(1000);

                var received = NetMessage.DeserializeFromJson(receiving.Result);

                Assert.That(received, Is.EqualTo(message));
            }
            await Task.Run(Check);
        }

        [Test]
        public async Task ReceivingTest()
        {
            static void Check()
            {
                ushort port = 12345;
                NetMQMessageSource ms;

                while (true)
                {
                    try
                    {
                        ms = new NetMQMessageSource(port);
                        break;
                    }
                    catch
                    {
                        port++;
                    }
                }

                var localEP = ms.ListeningEndPoint;

                var message = new NetMessage()
                {
                    UserFrom = "User1",
                    UserTo = "User2",
                    Text = "Hello!"
                };

                var sender = new RequestSocket();
                
                
                NetMessage received = new();
                sender.Connect(NetMQMessageSource.GetConnectionString(localEP));
                sender.SendFrame(message.SerializeToJson());
                ms.StartReceivingAsync((x, y) => { received = x; });
                Thread.Sleep(1000);
                Assert.That(received, Is.EqualTo(message));
            }
            await Task.Run(Check);
        }

        [Test]
        public async Task InBoxTest()
        {
            static void Check()
            {
                NetMQMessageSource NewSource()
                {
                    ushort port = 12345;
                    NetMQMessageSource result;
                    while (true)
                    {
                        try
                        {
                            result = new NetMQMessageSource(port);
                            break;
                        }
                        catch { port++; }
                    }
                    return result;
                }

                var ms1 = NewSource();
                var ms2 = NewSource();

                NetMessage message = new()
                {
                    UserFrom = "User1",
                    UserTo = "User2",
                    Text = "Hello!"
                };

                var receiving = ms1.StartReceivingAsync();
                Thread.Sleep(1000);
                var sending = ms2.SendAsync(message, ms1.ListeningEndPoint);

                sending.Wait(1000);
                Thread.Sleep(1000);
                
                Assert.That(ms1.InBox.Count, Is.EqualTo(1));
                Assert.That(ms1.InBox.Last().Item1, Is.EqualTo(message));
            }
            await Task.Run(Check);
        }
    }
}
