using Core.Messages.Messages;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Native.Requests
{
    [DataContract]
    public class NativeStorageDeleteAllRequest:TicketedMessageBase
    {
        public NativeStorageDeleteAllRequest() : base(MessageTypes.NativeStorageDeleteAll)
        {

        }
    }
}
