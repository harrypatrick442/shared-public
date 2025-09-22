namespace Native.Permissions
{
    public class RequestPermissionResult
    {
        public DesiredPermission DesiredPermission { get; }
        public bool Granted { get; }
        public RequestPermissionResult(DesiredPermission desiredPermission, bool granted) { 
            DesiredPermission = desiredPermission;    
            Granted = granted;  
        }
    }
}