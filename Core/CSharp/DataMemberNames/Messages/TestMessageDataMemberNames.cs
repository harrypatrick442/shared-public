using MessageTypes.Attributes;

namespace Core.DataMemberNames.Messages
{
    [MessageType(global::MessageTypes.MessageTypes.Test)]
    public class TestMessageDataMemberNames
    {
        public const string Value = "v";
    }
}