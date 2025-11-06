
namespace ConfigurationCore
{
    public interface IUrlsConfiguration
    {
        public string LogServerLogSession { get; }
        public string LogServerLogBreadcrumb { get; }
        public string LogServerLogError { get; }
    }
}
