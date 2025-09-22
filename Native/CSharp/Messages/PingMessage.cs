using Core.Messages.Messages;
using System.Runtime.Serialization;
namespace Native.Messages
{
    [DataContract]
	public class PingMessage : TypedMessageBase
    {
        public PingMessage()
        {
            _Type = MessageTypes.MessageTypes.Ping;
        }
    }
}
