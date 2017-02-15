using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using CommonLibrary.Messages;

namespace SimpleClient
{
    class Client
    {
        private readonly string _ip;
        private readonly int _port;
        private readonly Guid _id;

        private TcpClient _client;
        private NetworkStream _stream;
        private StreamReader _reader;
        private StreamWriter _writer;
        private Timer _timer;
        private string _currentRoomName;
        private readonly JsonSerializer _jsonSerializer;

        public Client(string ip, int port)
        {
            _ip = ip;
            _port = port;
            _id = Guid.NewGuid();
            _jsonSerializer = new JsonSerializer();
        }

        public void SendMessageToServer()
        {
            try
            {
                _client = new TcpClient(_ip, _port);
                _stream = _client.GetStream();
                _reader = new StreamReader(_stream);
                _writer = new StreamWriter(_stream) {AutoFlush = true};
                _currentRoomName = Console.ReadLine();
                if (_currentRoomName != null)
                {
                    EnterToRoom();
                }
                _reader.ReadLine();
                ThreadPool.QueueUserWorkItem(ListenServer, null);
                
                _timer = new Timer(TimerTick, new object(), 0, 100);
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                CloseConnection();
            }
        }

        private void EnterToRoom()
        {
            TextWriter writer = new StringWriter();
            _jsonSerializer.Serialize(writer, new EntryRoomMessage(_id, _currentRoomName));
            ServerMessage serverMessage = new ServerMessage {Content = writer.ToString(), Name = EntryRoomMessage.Name};
            writer = new StringWriter();
            _jsonSerializer.Serialize(writer, serverMessage);
            _writer.WriteLine(writer.ToString());
        }

        private void ListenServer(object state)
        {
            try
            {
                _reader.ReadLine();
                while (true)
                {
                    var message = _reader.ReadLine();
                    ServerMessage serverMessage = JsonConvert.DeserializeObject<ServerMessage>(message);
                    switch (serverMessage.Name)
                    {
                        case SendRoomMessage.Name:
                        {
                            SendRoomMessage roomMessage =
                                JsonConvert.DeserializeObject<SendRoomMessage>(serverMessage.Content);
                            if (roomMessage.Id != _id)
                            {
                                Console.WriteLine("Message from {0}:{1}", roomMessage.RoomName, roomMessage.Message);
                            }
                            break;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        private void TimerTick(object state)
        {
            try
            {
                TextWriter writer = new StringWriter();
                _jsonSerializer.Serialize(writer, new SendRoomMessage(_id, _currentRoomName, $"message from {_id}"));
                ServerMessage serverMessage = new ServerMessage { Content = writer.ToString(), Name = SendRoomMessage.Name };
                writer = new StringWriter();
                _jsonSerializer.Serialize(writer, serverMessage);
                _writer.WriteLine(writer.ToString());
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        public void CloseConnection()
        {
            SendCloseConnection();
            _reader.Dispose();
            _writer.Dispose();
            _stream.Dispose();
            _client.Close();
            _timer.Dispose();
        }

        private void SendCloseConnection()
        {
            TextWriter writer = new StringWriter();
            _jsonSerializer.Serialize(writer, new LeaveRoomMessage(_id, _currentRoomName));
            ServerMessage serverMessage = new ServerMessage { Content = writer.ToString(), Name = LeaveRoomMessage.Name };
            writer = new StringWriter();
            _jsonSerializer.Serialize(writer, serverMessage);
            _writer.WriteLine(writer.ToString());
        }
    }
}
