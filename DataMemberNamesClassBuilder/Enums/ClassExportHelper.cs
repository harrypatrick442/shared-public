

using System;

namespace DataMemberNamesClassBuilder
{
    [Flags]
    public enum ClassExportType
    {
        Request=1,
        Response=2,
        Message=4
    }
    public static class ClassExportTypeExtensions
    {
        public static bool IsRequest(this ClassExportType type)
        {
            return (type&ClassExportType.Request)>0;
        }
        public static bool IsResponse(this ClassExportType type)
        {
            return (type&ClassExportType.Response)>0;
        }
        public static bool IsMessage(this ClassExportType type)
        {
            return (type&ClassExportType.Message)>0;
        }
    }
    public static class ClassExportTypeHelper {
        public static ClassExportType FromClassName(string className)
        {

            string classNameLower = className.ToLower();
            bool isRequest = classNameLower.Contains("request");
            bool isResponse = classNameLower.Contains("response");
            if (isRequest)
            {
                if (isResponse)
                    throw new System.Exception($"The class named {className} cannot be both a request and a response");
                return ClassExportType.Request;
            }
            if (isResponse)
            {
                return ClassExportType.Response;
            }
            return ClassExportType.Message;
        }
    }
}
