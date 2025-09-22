

namespace DataMemberNamesClassBuilder
{
    public class ConstantsClassMember
    {
        public string Name { get; }
        public object Value { get; }
        public ConstantsClassMember(string name, object value) {
            Name = name;
            Value = value;
        }
    }
}
