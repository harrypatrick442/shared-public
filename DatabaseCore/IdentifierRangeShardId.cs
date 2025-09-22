using Core.Pool;
using Microsoft.Data.Sqlite;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Text;
namespace Database
{
    [DataContract]
    public class IdentifierRangeShardId
    {
        [JsonPropertyName("f")]
        [JsonInclude]
        [DataMember(Name ="f")]
        public long FromInclusive { get; protected set; }
        [JsonPropertyName("t")]
        [JsonInclude]
        [DataMember(Name = "t")]
        public long ToExclusive { get; protected set; }
        [JsonPropertyName("i")]
        [JsonInclude]
        [DataMember(Name = "i")]
        public int ShardId { get; protected set; }
        public IdentifierRangeShardId(long fromInclusive, long toExclusive, int shardId) {
            FromInclusive = fromInclusive;
            ToExclusive = toExclusive;
            ShardId = shardId;
        }
        protected IdentifierRangeShardId() { }
    }
}
