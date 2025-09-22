namespace Native.Permissions
{
    public class DesiredPermission
    {
        public string Permission { get; }
        public bool Optional{ get; }
        public string? UserFriendlyName { get; }
        public DesiredPermission(string permission, bool optional, string userFriendlyName = null) {
            Permission = permission;
            Optional = optional;
            UserFriendlyName = userFriendlyName;
        }

        public override bool Equals(object obj)
        {
            return obj is DesiredPermission permission &&
                   Permission == permission.Permission;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Permission);
        }
    }
}