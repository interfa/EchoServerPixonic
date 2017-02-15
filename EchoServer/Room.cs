using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace EchoServer
{
    public class Room
    {
        public string Name { get; private set; }
        private readonly ConcurrentDictionary<string, StreamWriter> _users;
        
        public DateTime LastTimeActivity { get; private set; }

        public Room(string name)
        {
            Name = name;
            _users = new ConcurrentDictionary<string, StreamWriter>();
        }

        internal void EntryRoom(KeyValuePair<string, StreamWriter> player, DateTime serverTime)
        {
            _users.TryAdd(player.Key, player.Value);
            LastTimeActivity = serverTime;
        }

        internal void SendMessage(string playerId, string message, DateTime serverTime)
        {
            foreach (var user in _users)
            {
                if (user.Key == playerId)
                {
                    continue;
                }
                user.Value.WriteLine(message);
            }
            LastTimeActivity = serverTime;
        }

        internal void RemoveUser(string playerId)
        {
            StreamWriter writer;
            _users.TryRemove(playerId, out writer);
            writer.Dispose();
        }
    }
}
