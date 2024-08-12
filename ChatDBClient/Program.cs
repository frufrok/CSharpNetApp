using System.Net;
using ChatDBNet.Message;
using ChatDBNet.Interfaces;
using ChatDBNet.Udp;

namespace ChatDBClient
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var serverIPEndPoint = new IPEndPoint(UdpMessageSource.GetLocalIPAddress(), 12345);
            if (args.Length == 0)
            {
                var ms = new UdpMessageSource(12346);
                new Client("Azat", serverIPEndPoint, ms).Run();
            }
            else if (args.Length == 1)
            {
                var ms = new UdpMessageSource(12346);
                new Client(args[0], serverIPEndPoint, ms).Run();
            }
            else if (args.Length == 2)
            {
                if (ushort.TryParse(args[1], out ushort port))
                {
                    var ms = new UdpMessageSource(port);
                    new Client(args[0], serverIPEndPoint, ms).Run();
                }
                else
                {
                    Console.WriteLine($"Не удалось преобразовать строку {args[1]} в номер порта.");
                }
            }
            else
            {
                Console.WriteLine("Запустите клиент с использованием двух аргументов: имени пользователя и номера порта.");
            }
        }
    }
}
