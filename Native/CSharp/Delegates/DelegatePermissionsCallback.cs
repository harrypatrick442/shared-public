using Native.Permissions;

namespace NativeAndroid.Permissions
{
    public delegate void DelegatePermissionsCallback(bool allGranted, RequestPermissionResult[] requestPermissionResults);
}