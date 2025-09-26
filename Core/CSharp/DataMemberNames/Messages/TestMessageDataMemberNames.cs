using MessageTypes.Attributes;

namespace Core.DataMemberNames.Messages
{
    [MessageType(MessageTypes.Test)]
    public class TestMessageDataMemberNames
    {
        public const string Value = "v";
    }
}