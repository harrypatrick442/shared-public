using MessageTypes.Attributes;
using CoreNativeAndBackend.DataMemberNames.Messages;

namespace Native.DataMemberNames.Requests
{
    [MessageType(MessageTypes.NativeShowSaveFilePicker)]
    public static class NativeShowSaveFilePickerRequestDataMemberNames
    {
        [DataMemberNamesClass(typeof(FileInfoDataMemberNames), isArray: false)]
        public const string FileInfo = "f";
    }
}