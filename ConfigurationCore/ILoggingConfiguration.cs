using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationCore
{
    public interface ILoggingConfiguration
    {
        public string LogServerLogErrorUrl { get; }
        public string LogServerLogBreadcrumbUrl { get; }
        public string LogServerLogSessionUrl { get; }
    }
}
