using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationCore
{
    public interface IKeysConfiguration
    {
        public string KeyPathOpenSSH { get; }
        public string KeyPath { get; }
    }
}
