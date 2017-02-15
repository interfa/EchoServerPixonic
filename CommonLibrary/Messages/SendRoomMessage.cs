using System;

namespace CommonLibrary.Messages
{
    [Serializable]
    public class SendRoomMessage
    {
        public const string Name = "mes";

        public Guid Id { get; set; }
        public string RoomName { get; set; }
        public string Message { get; set; }

        public SendRoomMessage(Guid id, string roomName, string message)
        {
            Id = id;
            RoomName = roomName;
            Message = message;
        }
    }
}
