using System;

namespace CommonLibrary.Messages
{
    [Serializable]
    public class LeaveRoomMessage
    {
        public const string Name = "stp";

        public Guid Id { get; set; }
        public string RoomName { get; set; }

        public LeaveRoomMessage(Guid id, string roomName)
        {
            Id = id;
            RoomName = roomName;
        }
    }
}
