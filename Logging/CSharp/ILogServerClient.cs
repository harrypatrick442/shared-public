using Core.Enums;
using System;

namespace Logging
{
    public interface ILogServerClient
    {
        void Breadcrumb(BreadcrumbType type, string description, string value);
        void Error(Exception ex);
        void Error(string message);
    }
}