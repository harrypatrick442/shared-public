using Core.DataMemberNames;
using MessageTypes.Internal;
using System.Net.NetworkInformation;

namespace Native
{
    public class MessageTypes
    {
    public const string Type = MessageTypeDataMemberName.Value,
        Ping = "p",
        ConsoleMessage = "cmsg",
        NativeReadyMessage = "nr",
        NativeNewClientToClientChannel = "nnctcc",
        NativeDisposeClientToClientChannel = "ndctcc",
        NativeSendFile = "nsf",
        NativeReceiveFile = "nrf",
        NativeProgress = "np",
        NativeError = "ne",
        NativePickFile = "npf",
        NativeCloseFile = "ncf",
        NativeShowSaveFilePicker = "nsaf",
        NativeGotNewToken = "ngnt",
        NativePlatform = "npt",
        NativeDownloadFile = "ndf",
        NativeOpenDirectory = "nod",
        NativePermissionsUpdate = "npu",
        NativeRequestPermissions = "nrp",
        NativeReloadPage = "nre",
        NativeStorageGetString = "nsgs",
        NativeStorageSetString = "nsss",
        NativeStorageDeleteAll = "nsda";
    }
}