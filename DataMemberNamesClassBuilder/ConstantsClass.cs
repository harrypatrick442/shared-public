   
namespace DataMemberNamesClassBuilder
{
    public class ConstantsClass
    {
        public string ClassName { get; }
        public ConstantsClassMember[] Members { get; }
        public ConstantsClass(string className, ConstantsClassMember[] members) {
            ClassName = className;
            Members = members;
        }
    }
}
