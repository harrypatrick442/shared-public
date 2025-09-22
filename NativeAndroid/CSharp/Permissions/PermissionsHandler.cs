
using NativeAndroid.Permissions;
using Android;
using NativeAndroid.Interfaces;
using Android.App;
using Core.Exceptions;
using Native.Permissions;

namespace NativeAndroid.Permissions{
    public class PermissionsHandler<TActivity>: PermissionsHandler
            where TActivity : Activity, IRequestPermissionsResultSource
    {
        private DesiredPermission[] _DesiredPermissions;
        private TActivity _Activity;
        private string _ApplicationName;
        private bool _IsRequestingPermissions = false;
        public PermissionsHandler(TActivity activity, 
            DesiredPermission[] desiredPermissions, string applicationName) {
            _Activity = activity;
            _DesiredPermissions = desiredPermissions;
            _ApplicationName = applicationName;
        }
        public override void Handle(DelegatePermissionsCallback? callback = null) {
            _Activity.RunOnUiThread(()=>{
                if (_IsRequestingPermissions) return;
                _IsRequestingPermissions = true;
                PermissionsHelper.CheckAskAndRequest(
                    _Activity,
                    _DesiredPermissions,
                    _ApplicationName,
                    (hasAllRequired, requestPermissionResults) => {
                        _IsRequestingPermissions = false;
                        callback?.Invoke(hasAllRequired, requestPermissionResults);
                    }
                );
            });
        }
        public override bool HasAllRequired()
        {
            return PermissionsHelper.HasAllRequired(_Activity, _DesiredPermissions);
        }
    }
    public abstract class PermissionsHandler: IPermissionsHandler
    {
        public abstract void Handle(DelegatePermissionsCallback? callback = null);
        public abstract bool HasAllRequired();
    }

}