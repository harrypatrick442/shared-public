
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;

namespace NativeAndroid.Delegates
{
    public delegate void DelegateHandleRequestPermissionsResult(int requestCode,
        string[] permissions, [GeneratedEnum] Permission[] grantResults);
}