using ChatDBNet.Message;
using ChatDBNet.Udp;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatDBTest
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class UdpMessageSourceTest
    {
        [Test]
        public async Task SendingUdpClientIsNotNull()
        {
            static void Check()
            {
                var ms = new UdpMessageSource();
                Assert.That(ms.SendingUdpClient, Is.Not.Null);
            }
            await Task.Run(Check);
        }

        [Test]
        public async Task ListeningUdpClientIsNotNull()
        {
            static void Check()
            {
                var ms = new UdpMessageSource();
                Assert.That(ms.ListeningUdpClient, Is.Not.Null);
            }
            await Task.Run(Check);
        }

        [Test]
        public async Task ListeningUdpClientHasGivenPort()
        {
            static void Check()
            {
                ushort port;
                using (var tempUdp = new UdpMessageSource())
                {
                    port = tempUdp.ListeningPort;
                }
                var ms = new UdpMessageSource(port);
                Assert.That(ms.ListeningPort, Is.EqualTo(port));
            }
            await Task.Run(Check);
        }

        [Test]
        public async Task SendingTest()
        {
            static void Check()
            {
                var receiver = new UdpClient();
                IPEndPoint localEP;
                ushort port = 12345;
                while(true)
                {
                    try
                    {
                        localEP = new IPEndPoint(UdpMessageSource.GetLocalIPAddress(), port);
                        receiver.Client.Bind(localEP);
                        break;
                    }
                    catch
                    {
                        port++;
                    }
                }
                
                IPEndPoint? remote = new IPEndPoint(IPAddress.Any, 0);
                byte[] result;
                
                var ms = new UdpMessageSource();

                NetMessage message = new NetMessage()
                {
                    UserFrom = "User1",
                    UserTo = "User2",
                    Text = "Hello!"
                };

                var receiving = Task.Run(() => result = receiver.Receive(ref remote));
                var sending = ms.SendAsync(message, localEP);

                sending.Wait(1000);
                receiving.Wait(1000);
                
                var received = NetMessage.DeserializeFromJson(Encoding.UTF8.GetString(receiving.Result));

                Assert.That(received, Is.EqualTo(message));
            }
            await Task.Run(Check);
        }

        [Test]
        public async Task ReceivingTest()
        {
            static void Check()
            {
                var sender = new UdpClient();
                UdpMessageSource ms;

                ushort port = 12345;
                while (true)
                {
                    try
                    {
                        ms = new(port);
                        break;
                    }
                    catch
                    {
                        port++;
                    }
                }
                
                NetMessage message = new NetMessage()
                {
                    UserFrom = "User1",
                    UserTo = "User2",
                    Text = "Hello!"
                };
                var data = Encoding.UTF8.GetBytes(message.SerializeToJson());

                NetMessage received = new();
                var receiving = ms.StartReceivingAsync((x, y) => { received = x; });
                var sending = sender.SendAsync(data, data.Length, ms.ListeningEndPoint);

                sending.Wait(1000);
                receiving.Wait(1000);

                Assert.That(received, Is.EqualTo(message));
            }
            await Task.Run(Check);
        }

        [Test]
        public async Task InBoxTest()
        {
            static void Check()
            {
                UdpMessageSource NewSource()
                {
                    ushort port = 12345;
                    UdpMessageSource result;
                    while (true)
                    {
                        try
                        {
                            result = new UdpMessageSource(port);
                            break;
                        }
                        catch { port++; }
                    }
                    return result;
                }

                UdpMessageSource ms1 = NewSource();
                UdpMessageSource ms2 = NewSource();

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
