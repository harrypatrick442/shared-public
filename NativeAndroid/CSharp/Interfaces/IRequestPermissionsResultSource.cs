using NativeAndroid.Delegates;
using NativeAndroid.Permissions;

namespace NativeAndroid.Interfaces
{
    public interface IRequestPermissionsResultSource {
        public void AddHandler(int requestCode, DelegateHandleRequestPermissionsResult handler);
    }
}