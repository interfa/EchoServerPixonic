using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CommonLibrary.Messages;
using Newtonsoft.Json;

namespace EchoServer
{
    class Server
    {
        private readonly string _ip;
        private readonly int _port;
        private readonly RoomManager _roomManager;

        public Server(string ip, int port)
        {
            _ip = ip;
            _port = port;
            _roomManager = new RoomManager();
        }

        public void Start()
        {
            try
            {
                IPAddress localAddress = IPAddress.Parse(_ip);
                TcpListener listener = new TcpListener(localAddress, _port);

                listener.Start();
                Console.WriteLine("Server wait {0}", listener.LocalEndpoint);

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    var stream = client.GetStream();
                    ThreadPool.QueueUserWorkItem(ClientCallback, stream);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void ClientCallback(object state)
        {
            var stream = (NetworkStream)state;
            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream) { AutoFlush = true };
            try
            {
                while (true)
                {
                    var receiveMessage = reader.ReadLine();
                    if (receiveMessage != null)
                    {
                        var baseMessage = JsonConvert.DeserializeObject<ServerMessage>(receiveMessage);
                        switch (baseMessage.Name)
                        {
                            case EntryRoomMessage.Name:
                                ConnectToServer(baseMessage.Content, writer);
                                break;
                            case SendRoomMessage.Name:
                                SendMessageToServer(receiveMessage, baseMessage.Content);
                                break;
                            case LeaveRoomMessage.Name:
                                DisconnectFromServer(baseMessage.Content);
                                return;
                        }
                        
                        writer.WriteLine(receiveMessage);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            finally
            {
                writer.Dispose();
                reader.Dispose();
                stream.Close();
            }
        }
        
        private void ConnectToServer(string content, StreamWriter writer)
        {
            var message = JsonConvert.DeserializeObject<EntryRoomMessage>(content);
            _roomManager.EntryRoom(new KeyValuePair<string, StreamWriter>(message.Id.ToString(), writer), message.RoomName);
            Console.WriteLine("{2} ConnectToServer {0}, {1}", message.Id, message.RoomName, DateTime.UtcNow);
        }

        private void SendMessageToServer(string baseMessage, string content)
        {
            var message = JsonConvert.DeserializeObject<SendRoomMessage>(content);
            _roomManager.SendMessage(message.Id.ToString(), message.RoomName, baseMessage);
            //Console.WriteLine("SendMessageToServer {0}, {1}", message.RoomName, message.Message);
        }

        private void DisconnectFromServer(string content)
        {
            var message = JsonConvert.DeserializeObject<LeaveRoomMessage>(content);
            _roomManager.LeaveRoom(message.RoomName, message.Id);
            Console.WriteLine("{2} DisconnectFromServer {0}, {1}", message.Id, message.RoomName, DateTime.UtcNow);
        }
    }
}
