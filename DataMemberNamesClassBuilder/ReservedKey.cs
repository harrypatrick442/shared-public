using MessageTypes.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMemberNamesClassBuilder
{
    public class ReservedKey
    {
        public string Value { get; }    
        public Type[] ActualTypesAllowedToUse { get; }
        public bool IsAllowedClass(Type actualType) {
            return ActualTypesAllowedToUse.Contains(actualType);
        }
        public ReservedKey(string value, params Type[] actualTypesAllowedToUse) {
            Value = value;
            ActualTypesAllowedToUse = actualTypesAllowedToUse;
        }
    }
}
