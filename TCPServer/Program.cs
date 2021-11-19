using System;
using System.Text;

namespace TcpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();

            server.Bind("192.168.0.102", 2002);
            server.StartAccept();

            while (true)
            {
                System.Threading.Thread.Sleep(100);
                server.Test();    //2 запускаю мониторинг подключенных клиентов
            }
            server.Close();
        }
    }
}
