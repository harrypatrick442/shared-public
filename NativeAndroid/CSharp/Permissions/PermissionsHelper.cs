
using Android.App;
using Java.Security;
using Native.Permissions;
using NativeAndroid.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Permission = Android.Content.PM.Permission;

namespace NativeAndroid.Permissions
{
    public static class PermissionsHelper
    {
        public static void CheckAskAndRequest<TActivity> (TActivity activity,
            DesiredPermission[] permissions, string applicationName,
            DelegatePermissionsCallback callback) 
            where TActivity : Activity, IRequestPermissionsResultSource
        {

            DesiredPermission[] permissionsToRequest = permissions
                .Where(p => !IsPermissionGranted(p.Permission, activity)).ToArray();
            DesiredPermission[] permissionsAlreadyHas = permissions
                .Where(p => !permissionsToRequest.Contains(p)).ToArray();
            if (permissionsToRequest.Count() < 1)
            {
                callback?.Invoke(true, permissionsAlreadyHas
                    .Select(p => new RequestPermissionResult(p, true))
                    .ToArray());
                return;
            }
            DesiredPermission[] permissionsToAskForWithDialog = permissionsToRequest.Where(
                p => activity.ShouldShowRequestPermissionRationale(p.Permission)).ToArray();
            DesiredPermission[] permissionsToNotAskFor = permissionsToRequest.Where(
                p => !permissionsToAskForWithDialog.Contains(p)).ToArray();
            bool needsToAsk = permissionsToAskForWithDialog.Count()>0;
            Action<bool> afterDialogIfNeeded = Get_AfterDialogIfNeeded(
                activity, permissionsToRequest, permissionsToAskForWithDialog,
                permissionsToNotAskFor, callback);
            if (needsToAsk)
            {
                ShowAlertDialog(activity,
                    permissionsToAskForWithDialog, applicationName, afterDialogIfNeeded);
                return;
            }
            afterDialogIfNeeded(true);
        }
        public static bool HasAllRequired<TActivity>(TActivity activity, DesiredPermission[] permissions)
            where TActivity : Activity, IRequestPermissionsResultSource
        {
            return !permissions.Where(p => !p.Optional)
                .Where(p=>!IsPermissionGranted(p.Permission, activity))
                .Any();
        }
        private static bool IsPermissionGranted<TActivity>(string permission, TActivity activity)
            where TActivity : Activity, IRequestPermissionsResultSource
        {
            bool value =  activity
                .CheckCallingOrSelfPermission(permission).Equals(Permission.Granted);
            return value;
        }
        private static Action<bool> Get_AfterDialogIfNeeded<TActivity>(
            TActivity activity, DesiredPermission[] permissionsToRequest,
            DesiredPermission[] permissionsToAskForWithDialog,
            DesiredPermission[] permissionsToNotAskFor,
            DelegatePermissionsCallback callback = null)
            where TActivity : Activity, IRequestPermissionsResultSource
        {
            return (acceptedOrDidntNeedToAsk) =>
            {
                if (acceptedOrDidntNeedToAsk)
                {

                    MakePermissionsRequest(activity, permissionsToRequest,
                        (results) =>
                        {
                            callback?.Invoke(AllNonOptionalPermissionsAreGranted(results),
                                results.ToArray());
                        });
                    return;
                }
                MakePermissionsRequest(
                    activity, permissionsToNotAskFor, (results) =>
                    {
                        results = results.Concat(
                            permissionsToAskForWithDialog
                            .Select(p => new RequestPermissionResult(p, false))).ToArray();
                        callback?.Invoke(AllNonOptionalPermissionsAreGranted(results),
                            results.ToArray());
                    });
            };
        }
        private static bool AllNonOptionalPermissionsAreGranted(IEnumerable<RequestPermissionResult>results)
        {
            return !results.Where(r => (!r.Granted) && (!r.DesiredPermission.Optional)).Any();
        }
        public static void ShowAlertDialog<TActivity>(TActivity activity, DesiredPermission[] permissions,
            string applicationName, Action<bool> callback)
            where TActivity : Activity, IRequestPermissionsResultSource
        {
            AlertDialog.Builder alertDialogBuilder = new AlertDialog.Builder(activity);
            applicationName = string.IsNullOrEmpty(applicationName) ? "This application" : applicationName;
            alertDialogBuilder.SetMessage(GetMessage(applicationName, permissions));
            alertDialogBuilder.SetPositiveButton("Yes", (sender, dialogClickEventArgs) =>
            {
                callback(true);
            });
            alertDialogBuilder.SetNegativeButton("No", (sender, dialogClickEventArgs) =>
            {
                callback(false);
            });
            AlertDialog alertDialog = alertDialogBuilder.Create();
            alertDialog.Show();
        }
        private static string GetMessage(string applicationName, IEnumerable<DesiredPermission> desiredPermission) {
            IEnumerable<DesiredPermission> required = desiredPermission.Where(d => !d.Optional);
            IEnumerable<DesiredPermission> optional = desiredPermission
                    .Where(p => p.Optional);
            string optionalNames = string.Join(",", optional.Select(p => GetFriendlyPermissionName(p, false)));
            if (!required.Any()){
                return $"{applicationName} would like the following optional permissions: {optionalNames}";
            }
            string requiredNames = string.Join(",", required.Select(p => GetFriendlyPermissionName(p, false)));
            string requiredMessage = $"{applicationName} requires the following permissions to run properly: {requiredNames}";
            if (!string.IsNullOrEmpty(optionalNames))
                requiredMessage += $". And optionally: {optionalNames}";
            return requiredMessage;
        }
        private static void MakePermissionsRequest<TActivity>(
            TActivity activity,
            DesiredPermission[] permissionsToRequest,
            Action<RequestPermissionResult[]> callback)
            where TActivity : Activity, IRequestPermissionsResultSource
        {
            int requestCode = Constants.Android.PERMISSIONS_REQUEST_CODE;
            activity.AddHandler(requestCode, (requestCode, permissions, grantResults) => {
                List<RequestPermissionResult> requestPermissionResult = new List<RequestPermissionResult>();
                Dictionary<string, bool> mapPermissionToGranted = new Dictionary<string, bool>();
                for (int i=0; i < permissions.Count(); i++ ){
                    mapPermissionToGranted[permissions[i]] = grantResults[i].Equals(Permission.Granted);
                }
                foreach (DesiredPermission permissionRequested in permissionsToRequest) {
                    if (!mapPermissionToGranted.TryGetValue(permissionRequested.Permission, out bool granted)) {
                        granted = IsPermissionGranted(permissionRequested.Permission, activity);
                    }
                    requestPermissionResult.Add(new RequestPermissionResult(permissionRequested, granted));
                }
                callback(requestPermissionResult.ToArray());
            });
            activity.RequestPermissions(permissionsToRequest.Select(d=>d.Permission).ToArray(), requestCode);
        }
        private static string GetFriendlyPermissionName(DesiredPermission desiredPermission, bool includeOptional) {
            string friendlyName;
            if (!string.IsNullOrEmpty(desiredPermission.UserFriendlyName))
                friendlyName = desiredPermission.UserFriendlyName;
            else 
                friendlyName = GetFriendlyPermissionName(desiredPermission.Permission);
            if (desiredPermission.Optional&& includeOptional) {
                return $"{friendlyName} (optional)";
            }
            return friendlyName;
        }

        private static string GetFriendlyPermissionName(string permission)
        {
            switch (permission)
            {
                case "android.permission.ACCESS_NETWORK_STATE":
                case "android.permission.INTERNET":
                    return "internet";
                case "android.permission.WRITE_EXTERNAL_STORAGE":
                    return "write files";
                case "android.permission.READ_CLIPBOARD":
                    return "read clipboard";
                case "android.permission.POST_NOTIFICATIONS":
                    return "notifications";
            }
            int lastFullStop = permission.LastIndexOf(".");
            if (lastFullStop < 0) return permission;
            int firstIndexEnding = lastFullStop + 1;
            string ending = permission.Substring(firstIndexEnding, permission.Length - firstIndexEnding);
            string roughPermissionName = string.Join(" ", ending.Split('_')
                .Select(s => s.ToLower()));
            return roughPermissionName;
        }
    }
}