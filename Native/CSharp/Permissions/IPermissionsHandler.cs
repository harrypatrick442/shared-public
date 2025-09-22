using NativeAndroid.Permissions;

namespace Native.Permissions
{
    public interface IPermissionsHandler
    {
        public void Handle(DelegatePermissionsCallback? callback = null);
    }
}