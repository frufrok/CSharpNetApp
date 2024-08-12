using ChatDBServer.Connections;
using ChatDBNet.Message;
using ChatDB.Models;

namespace ChatDBTest
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class ChatDBResourceTest
    {
        private static string _connectionString =
            "Host=localhost;Port=5432;Database=ChatDB;" +
                "Username=postgres;Password=password";
        private void ClearDB()
        {
            using var context = new ChatDBContext();
            context.Messages.RemoveRange(context.Messages);
            context.Users.RemoveRange(context.Users);
            context.SaveChanges();
        }

        [SetUp]
        public void Setup()
        {
            ClearDB();
            using var context = new ChatDBContext();
        }

        [TearDown]
        public void Teardown()
        {
            ClearDB();
        }

        [Test]
        public async Task AddMessageTest()
        {
            static void Check()
            {
                var resource = new ChatDBResource();
                resource.AddUser("User1", out int user1ID);
                resource.AddUser("User2", out int user2ID);
                var msg = new NetMessage()
                {
                    UserFrom = "User1",
                    UserTo = "User2",
                    Text = "Hello!"
                };
                resource.AddMessage(msg, false, out var ID);
                Assert.That(ID, Is.AtLeast(0));
            }
            await Task.Run(Check);
        }

        [Test]
        public async Task AddUserTest()
        {
            static void Check()
            {
                var resource = new ChatDBResource();
                resource.AddUser("User3", out int user3ID);
                Assert.That(user3ID, Is.AtLeast(0));
            }
        }

        [Test]
        public async Task GetMessagesTest()
        {
            static void Check()
            {
                var resource = new ChatDBResource();
                resource.AddUser("User4", out int user4ID);
                resource.AddUser("User5", out int user5ID);

                var msg = new NetMessage()
                {
                    UserFrom = "User4",
                    UserTo = "User5",
                    Text = "Hello!"
                };

                resource.AddMessage(msg, true, out var ID);

                var dbmsg = resource.GetMessages(user5ID, false).First();

                Assert.That(dbmsg, Is.EqualTo(msg));
            }
            await Task.Run(Check);
        }

        [Test]
        public async Task GetUserIDTest()
        {
            static void Check()
            {
                var resource = new ChatDBResource();
                string nickname = "User6";
                resource.AddUser(nickname, out int userID);
                int id = resource.GetUserID(nickname);
                Assert.That(id, Is.EqualTo(userID));
            }
            await Task.Run(Check);
        }

        [Test]
        public async Task GetUsersTest()
        {
            static void Check()
            {
                var resource = new ChatDBResource();
                string user7 = "User7";
                string user8 = "User8";
                resource.AddUser(user7, out int user7ID);
                resource.AddUser(user8, out int user8ID);
                var dict = resource.GetUsers();
                Assert.Multiple(() =>
                {
                    Assert.That(dict[user7ID], Is.EqualTo(user7));
                    Assert.That(dict[user8ID], Is.EqualTo(user8));
                });
            }
            await Task.Run(Check);
        }
    }
}
