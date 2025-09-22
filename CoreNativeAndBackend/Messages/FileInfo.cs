using Core.Messages.Messages;
using CoreNativeAndBackend.DataMemberNames.Messages;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CoreNativeAndBackend.Messages
{
    [DataContract]
    public class FileInfo : TypedMessageBase
    {
        private long _Size;
        [JsonPropertyName(FileInfoDataMemberNames.Size)]
        [JsonInclude]
        [DataMember(Name = FileInfoDataMemberNames.Size)]
        public long Size { get { return _Size; } protected set { _Size = value; } }
        private string _Name;
        [JsonPropertyName(FileInfoDataMemberNames.Name)]
        [JsonInclude]
        [DataMember(Name = FileInfoDataMemberNames.Name)]
        public string Name { get { return _Name; } protected set { _Name = value; } }
        private string _FileType;
        [JsonPropertyName(FileInfoDataMemberNames.Type)]
        [JsonInclude]
        [DataMember(Name = FileInfoDataMemberNames.Type)]
        public string FileType { get { return _FileType; } protected set { _FileType = value; } }
        public FileInfo(long size, string name, string type)
        {
            _Size = size;
            _Name = name;
            _FileType = type;
        }
        protected FileInfo() { }
    }
}
