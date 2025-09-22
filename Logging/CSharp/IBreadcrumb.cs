using Core.Enums;

namespace Logging
{
    public interface IBreadcrumb
    {
        long AtClientUTC { get; }
        BreadcrumbType BreadcrumbType { get; }
        string Description { get; set; }
        long SessionId { get; }
        int TypeId { get; set; }
        string Value { get; set; }
    }
}