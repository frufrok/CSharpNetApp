using System.Net;
using ChatDBNet.Message;
using ChatDBNet.Interfaces;
using ChatDBServer.Interfaces;
using ChatDBServer.Services;
using ChatDBTest.Mock;
using Moq;

namespace ChatDBTest
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class ServerTest
    {
        [Test]
        public async Task RegistrationTest()
        {
            static void Check()
            {
                var db = new Mock<IDBResource>();
                var ms = new ServerMessageSourceMock();
                var server = new Server(ms, db.Object);
                Task.Run(() => server.Run());
                var ep = new IPEndPoint(IPAddress.Any, 0);
                var msg = NetMessage.CreateRegistration("User", ep);
                ms.InBox.Add((msg, ep));
                Thread.Sleep(1000);
                Assert.Multiple(() =>
                {
                    Assert.That(ms.OutBox.Count, Is.EqualTo(2));
                    Assert.That(ms.OutBox[0].Item1.MessageType, Is.EqualTo(MessageType.CONFIRMATION));
                    Assert.That(ms.OutBox[0].Item1.Text, Is.EqualTo(msg.GetHashCode().ToString()));
                    Assert.That(ms.OutBox[0].Item2.Equals(ep));
                    Assert.That(ms.OutBox[1].Item1.MessageType.Equals(MessageType.SIMPLE));
                    Assert.That(ms.OutBox[1].Item2.Equals(ep));
                });
                
            }
            await Task.Run(Check);
        }

        [Test]
        public async Task SimpleMessageTest()
        {
            static void Check()
            {
                var msg = new NetMessage() { UserFrom = "User", UserTo = "User", Text = "Hello!" };
                var db = new Mock<IDBResource>();
                db.Setup(x => x.GetUserID("User")).Returns(1);
                var ms = new ServerMessageSourceMock();
                var server = new Server(ms, db.Object);
                Task.Run(() => server.Run());

                var ep = new IPEndPoint(IPAddress.Any, 0);
                var reg = NetMessage.CreateRegistration("User", ep);
                ms.InBox.Add((reg, ep));
                ms.InBox.Add((msg, ep));
                Thread.Sleep(1000);
                Thread.Sleep(1000);
                Assert.Multiple(() =>
                {
                    Assert.That(ms.OutBox.Count, Is.EqualTo(4));
                    Assert.That(ms.OutBox[0].Item1.MessageType, Is.EqualTo(MessageType.CONFIRMATION));
                    Assert.That(ms.OutBox[0].Item1.Text, Is.EqualTo(reg.GetHashCode().ToString()));
                    Assert.That(ms.OutBox[0].Item2.Equals(ep));
                    Assert.That(ms.OutBox[1].Item1.MessageType.Equals(MessageType.SIMPLE));
                    Assert.That(ms.OutBox[1].Item2.Equals(ep));
                    Assert.That(ms.OutBox[2].Item1.MessageType, Is.EqualTo(MessageType.CONFIRMATION));
                    Assert.That(ms.OutBox[2].Item1.Text, Is.EqualTo(msg.GetHashCode().ToString()));
                    Assert.That(ms.OutBox[2].Item2.Equals(ep));
                    Assert.That(ms.OutBox[3].Item1.MessageType.Equals(MessageType.SIMPLE));
                    Assert.That(ms.OutBox[3].Item2.Equals(ep));
                });
            }
            await Task.Run(Check);
        }
    }
}
