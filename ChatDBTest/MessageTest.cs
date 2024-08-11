using ChatDBNet.Message;

namespace ChatDBTest
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class MessageTest
    {
        [Test]
        public async Task MessageIsUpToDate()
        {
            static void check()
            {
                var message = new NetMessage();
                var now = DateTime.Now;
                var elapsed = now - message.DateTime;
                Assert.That(elapsed.Milliseconds, Is.LessThan(5));
            }
            await Task.Run(check);
        }

        [Test]
        public async Task MessagesAreEquals()
        {
            static void check()
            {
                DateTime now = DateTime.Now;
                string text = "Hello";
                string user1 = "User1";
                string user2 = "User2";
                MessageType mtype = MessageType.SIMPLE;
                var message1 = new NetMessage()
                {
                    DateTime = now,
                    Text = text,
                    UserFrom = user1,
                    UserTo = user2,
                    MessageType = mtype
                };
                var message2 = new NetMessage()
                {
                    DateTime = now,
                    Text = text,
                    UserFrom = user1,
                    UserTo = user2,
                    MessageType = mtype
                };
                Assert.That(message1, Is.EqualTo(message2));
            }
            await Task.Run(check);
        }

        [Test]
        public async Task MessageCloneIsEquals()
        {
            static void check()
            {
                var message = new NetMessage()
                {
                    Text = "Hello!",
                    UserFrom = "User1",
                    UserTo = "User2",
                };
                var clone = (NetMessage)message.Clone();
                Assert.That(clone, Is.EqualTo(message));
            }
            await Task.Run(check);
        }
        
        [Test]
        public async Task MessagesAreNotEquals()
        {
            static void check()
            {
                var message = new NetMessage()
                {
                    MessageType = MessageType.SIMPLE,
                    Text = "Hello!",
                    UserFrom = "User1",
                    UserTo = "User2",
                };
                var clone1 = (NetMessage)message.Clone();
                var clone2 = (NetMessage)message.Clone();
                var clone3 = (NetMessage)message.Clone();
                var clone4 = (NetMessage)message.Clone();
                var clone5 = (NetMessage)message.Clone();

                clone1.MessageType = MessageType.LIST;
                clone2.UserFrom = "NewUser";
                clone3.UserTo = "NewUser";
                clone4.Text = "Hi!";
                clone5.DateTime = DateTime.MinValue;
                Assert.Multiple(() =>
                {
                    Assert.That(clone1, Is.Not.EqualTo(message));
                    Assert.That(clone2, Is.Not.EqualTo(message));
                    Assert.That(clone3, Is.Not.EqualTo(message));
                    Assert.That(clone4, Is.Not.EqualTo(message));
                    Assert.That(clone5, Is.Not.EqualTo(message));
                });
            }
            await Task.Run(check);
        }

        [Test]
        public async Task MessageSerialization()
        {
            static void check()
            {
                var message1 = new NetMessage() { Text = "Hello", UserFrom = "User1", UserTo = "User2" };
                var message2 = NetMessage.DeserializeFromJson(message1.SerializeToJson());
                Assert.That(message2, Is.Not.Null);
                Assert.That(message2, Is.EqualTo(message1));
            }
            await Task.Run(check);
        }
    }
}
