using ChatDBServer.Services;

namespace ChatDBTest
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class ServerTest
    {
        [Test]
        public async Task ServerIsInitialized()
        {
            await Task.Run(() => new Server());
        }
        [Test]
        public async Task ServerHasGivenPort()
        {
            await Task.Run(() =>
            {
                var server = new Server(12345);
            });
            
        }
    }
}
