using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace EchoServer
{
    public class RoomManager
    {
        private readonly ConcurrentDictionary<string, Room> _rooms;
        private DateTime _serverTime;
        private readonly Timer _timer;
        public const int TimerPeriod = 1000;
        public const int DeleteRoomTime = 60 * 1000;

        public RoomManager()
        {
            _rooms = new ConcurrentDictionary<string, Room>();
            _serverTime = DateTime.UtcNow;
            _timer = new Timer(ProcessRooms);
            _timer.Change(0, Timeout.Infinite);
        }

        private void ProcessRooms(object state)
        {
            List<Room> roomsForRemove = new List<Room>();
            foreach (var room in _rooms)
            {
                if ((_serverTime - room.Value.LastTimeActivity).TotalMilliseconds > DeleteRoomTime)
                {
                    roomsForRemove.Add(room.Value);
                }
            }
            foreach (Room room in roomsForRemove)
            {
                Room removeRoom;
                _rooms.TryRemove(room.Name, out removeRoom);
                Console.WriteLine("{0} RemoveRoom {1}", DateTime.UtcNow, removeRoom.Name);
            }
            int interval = (int)(DateTime.UtcNow - _serverTime).TotalMilliseconds;
            var dueTime = interval < TimerPeriod ? TimerPeriod - interval : 0;
            _serverTime = DateTime.UtcNow;
            _timer.Change(dueTime, Timeout.Infinite);
        }

        public void EntryRoom(KeyValuePair<string, StreamWriter> player, string roomName)
        {
            Room room = _rooms.GetOrAdd(roomName, new Room(roomName));
            room.EntryRoom(player, _serverTime);
            Console.WriteLine("{0} EntryRoom {1}", DateTime.UtcNow, room.Name);
        }

        internal void SendMessage(string playerId, string roomName, string message)
        {
            Room room;
            _rooms.TryGetValue(roomName, out room);
            room?.SendMessage(playerId, message, _serverTime);
        }

        internal void LeaveRoom(string roomName, Guid id)
        {
            Room room;
            _rooms.TryGetValue(roomName, out room);
            room?.RemoveUser(id.ToString());
        }
    }
}
