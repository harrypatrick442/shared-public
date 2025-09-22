using MessageTypes.Attributes;
using MessageTypes.Internal;

namespace WebAbstract.DataMemberNames.Interserver.Requests
{
    [MessageType(InterserverMessageTypes.GetLoadFactor)]
    public static class GetLoadFactorRequestDataMemberNames
    {
        public const string LoadFactorType = "t";
    }
}