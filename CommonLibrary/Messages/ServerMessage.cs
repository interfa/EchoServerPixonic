using System;

namespace CommonLibrary.Messages
{
    [Serializable]
    public class ServerMessage
    {
        public string Name { get; set; }
        public string Content { get; set; }
    }
}
