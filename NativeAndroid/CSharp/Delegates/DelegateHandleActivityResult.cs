
using Android.App;
using Android.Content;

namespace NativeAndroid.Delegates
{
    public delegate void DelegateHandleActivityResult(int requestCode, Result resultCode, Intent data);
}