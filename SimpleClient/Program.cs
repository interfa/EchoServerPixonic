using System;
using System.Configuration;
using System.Runtime.InteropServices;

namespace SimpleClient
{
    internal delegate void SignalHandler(ConsoleSignal consoleSignal);

    internal enum ConsoleSignal
    {
        CtrlC = 0,
        CtrlBreak = 1,
        Close = 2,
        LogOff = 5,
        Shutdown = 6
    }

    internal static class ConsoleHelper
    {
        [DllImport("Kernel32", EntryPoint = "SetConsoleCtrlHandler")]
        public static extern bool SetSignalHandler(SignalHandler handler, bool add);
    }

    class Program
    {
        private static SignalHandler _signalHandler;
        private static Client _client;

        static void Main()
        {
            _signalHandler += CloseApplication;
            ConsoleHelper.SetSignalHandler(_signalHandler, true);

            string ip = ConfigurationManager.AppSettings.Get("Ip");
            int port = Int32.Parse(ConfigurationManager.AppSettings.Get("Port"));

            _client = new Client(ip, port);
            Console.WriteLine("Write room name");
            _client.SendMessageToServer();
        }

        private static void CloseApplication(ConsoleSignal consoleSignal)
        {
            if (_client == null)
            {
                return;
            }
            if (consoleSignal == ConsoleSignal.Close)
            {
                _client.CloseConnection();
            }
        }
    }
}
