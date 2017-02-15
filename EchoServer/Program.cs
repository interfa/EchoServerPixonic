using System;
using System.Configuration;
namespace EchoServer
{
    class Program
    {

        static void Main()
        {
            string ip = ConfigurationManager.AppSettings.Get("Ip");
            int port = Int32.Parse(ConfigurationManager.AppSettings.Get("Port"));
            Server server = new Server(ip, port);
            server.RenameMePlease();
        }
    }
}
