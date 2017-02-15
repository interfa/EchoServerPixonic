using System;

namespace CommonLibrary.Messages
{
    [Serializable]
    public class EntryRoomMessage
    {
        public const string Name = "con";

        public Guid Id { get; set; }
        public string RoomName { get; set; }

        public EntryRoomMessage(Guid id, string roomName)
        {
            Id = id;
            RoomName = roomName;
        }
    }
}
