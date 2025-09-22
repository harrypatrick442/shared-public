using NativeAndroid.Delegates;

namespace NativeAndroid.Interfaces
{
    public interface IActivityResultSource
    {
        public void AddHandler(int requestCode, DelegateHandleActivityResult handler);
    }
}