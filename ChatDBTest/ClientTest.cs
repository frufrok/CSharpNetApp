using ChatDBNet.Message;
using ChatDBServer.Interfaces;
using ChatDBServer.Services;
using ChatDBClient;
using ChatDBTest.Mock;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ChatDBNet.Udp;

namespace ChatDBTest
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    internal class ClientTest
    {
        [Test]
        public async Task RegistrationTest()
        {
            static void Check()
            {
                var ms = new ClientMessageSourceMock();
                var serverIPEndPoint = new IPEndPoint(UdpMessageSource.GetLocalIPAddress(), 12345);
                var client = new Client("User", serverIPEndPoint, ms);
                Task.Run(() => client.Run());
                Thread.Sleep(1000);
                Assert.Multiple(() =>
                {
                    Assert.That(ms.OutBox.Count, Is.EqualTo(1));
                    Assert.That(ms.OutBox[0].Item1.MessageType, Is.EqualTo(MessageType.REGISTRATION));
                    Assert.That(ms.OutBox[0].Item1.UserFrom, Is.EqualTo("User"));
                    Assert.That(ms.OutBox[0].Item2.Equals(serverIPEndPoint));
                });
            }
            await Task.Run(Check);
        }
    }
}
