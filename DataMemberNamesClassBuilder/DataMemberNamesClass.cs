using System;
using System.Linq;
using System.Text;
using Core.Strings;
using MessageTypes.Attributes;
using System.Collections.Generic;
using System.CodeDom;
using AngleSharp.Io;
using Core.DataMemberNames;
using Core.Ticketing;
using BaseMessages.Constants;

namespace DataMemberNamesClassBuilder
{
    public class DataMemberNamesClass
    {
        private string _TypeName;
        public string TypeName { get { return _TypeName; } }
        private string _ClassName;
        public string ClassName { get { return _ClassName; } }
        private const string SUCCESS_VARIABLE_NAME = "s";
        private const string CPP_TYPE_PRIVATE_VARIABLE_NAME = "_tpe";

        public ClassExportType ClassExportType
        {
            get
            {
                return ClassExportTypeHelper.FromClassName(_ClassName);
            }
        }
        public bool IsRequest { get { return ClassExportType.IsRequest(); } }
        public bool IsResponse { get { return ClassExportType.IsResponse(); } }
        public bool IsMessage { get { return ClassExportType.IsMessage(); } }
        private MessageTypeAttribute _MessageTypeAttribute;
        private DataMemberFieldNameValueAttributes[] _DataMemberPropertyNameValuePairs;
        public DataMemberNamesClass(string className,
            DataMemberFieldNameValueAttributes[] dataMemberPropertyNameValuePairs,
            MessageTypeAttribute messageTypeAttribute) 
        {

            _TypeName = className;
            _ClassName = className.Replace("DataMemberNames", "");
            _DataMemberPropertyNameValuePairs = dataMemberPropertyNameValuePairs;
            _MessageTypeAttribute = messageTypeAttribute;
        }
        public void ToCPlusPlusClassStrings(Func<Type, DataMemberNamesClass> getDataMemberNamesClass, 
            out string hpp, out string cpp)
        {
            string classNameLower = _ClassName.ToLower();
            StringBuilder sbHpp = new StringBuilder();
            StringBuilder sbCpp = new StringBuilder();
            List<string> seenImports = new List<string> { _ClassName };
            StringBuilder sbImports = new StringBuilder();
            sbImports.AppendLine("#include \"../../cJSON/cJSON.h\"");
            sbImports.AppendLine("#include <memory>");
            Action<DataMemberNamesClass> addInclude = (dataMemberNamesClass) => {
                if (seenImports.IndexOf(dataMemberNamesClass.ClassName) >= 0)
                    return;
                sbImports.Append("#include \"./");
                sbImports.Append(dataMemberNamesClass.ClassName); ;
                sbImports.AppendLine(".hpp\"");
                seenImports.Add(dataMemberNamesClass.ClassName);
            };
            string[] lowerCamelCasePropertyNames = _DataMemberPropertyNameValuePairs
                .Select(d => StringHelper.LowerCamelCase(d.Name)).ToArray();
            string ifndefName = ClassName.ToUpper()+"_HPP";
            StringBuilder sbIfNDef = new StringBuilder();
            sbIfNDef.Append("#ifndef ");
            sbIfNDef.AppendLine(ifndefName);
            sbIfNDef.Append("#define ");
            sbIfNDef.AppendLine(ifndefName);
            sbIfNDef.AppendLine();
            VariableNameSource<DataMemberFieldNameValueAttributes>
                variableNameSource = new VariableNameSource<DataMemberFieldNameValueAttributes>();

            StringBuilder sbPrivateDataMembers = new StringBuilder();
            StringBuilder sbGetterSignatures = new StringBuilder();
            StringBuilder sbGetters = new StringBuilder();
            StringBuilder sbConstructorSignature = new StringBuilder();
            StringBuilder sbConstructor = new StringBuilder();
            StringBuilder sbConstructorPart2 = new StringBuilder();
            StringBuilder sbDeconstructorSignature = new StringBuilder();
            StringBuilder sbDeconstructor = new StringBuilder();
            StringBuilder sbToJSONSignature = new StringBuilder();
            StringBuilder sbFromJSONSignature = new StringBuilder();
            StringBuilder sbToJSON = new StringBuilder();
            StringBuilder sbFromJSON = new StringBuilder();
            StringBuilder sbFromJSONSecondPart = new StringBuilder();
            sbPrivateDataMembers.AppendLine("   private:");
            sbGetterSignatures.AppendLine("   public:");
            sbConstructorSignature.Append("        ");
            sbConstructorSignature.Append(ClassName);
            sbConstructorSignature.AppendLine("(");
            sbConstructor.Append(ClassName);
            sbConstructor.Append("::");
            sbConstructor.Append(ClassName);
            sbConstructor.AppendLine("(");
            sbDeconstructorSignature.Append("        ~");
            sbDeconstructorSignature.Append(_ClassName);
            sbDeconstructorSignature.AppendLine("();");

            sbDeconstructor.Append(ClassName);
            sbDeconstructor.Append("::~");
            sbDeconstructor.Append(ClassName);
            sbDeconstructor.AppendLine("(){");

            sbToJSONSignature.Append("        cJSON* toJSON() noexcept;");
            sbFromJSONSignature.Append("        static std::shared_ptr<");
            sbFromJSONSignature.Append(ClassName);
            sbFromJSONSignature.Append("> fromJSON(cJSON* j) noexcept;");

            sbToJSON.Append("cJSON* ");
            sbToJSON.Append(ClassName);
            sbToJSON.AppendLine("::toJSON(){");
            sbFromJSON.Append("std::shared_ptr<");
            sbFromJSON.Append(ClassName);
            sbFromJSON.Append("> ");
            sbFromJSON.Append(ClassName);
            sbFromJSON.AppendLine("::fromJSON(cJSON* j){");
            sbFromJSON.Append("    bool ");
            sbFromJSON.Append(SUCCESS_VARIABLE_NAME);
            sbFromJSON.AppendLine(" = true;");
            sbFromJSONSecondPart.Append("    return std::make_shared<");
            sbFromJSONSecondPart.Append(ClassName);
            sbFromJSONSecondPart.Append(">");
            sbFromJSONSecondPart.Append("(");
            sbToJSON.AppendLine("    cJSON *j = cJSON_CreateObject();");
            bool firstVariable = true;
            bool hadParameter = false;
            var propertyNameValuePairs = _DataMemberPropertyNameValuePairs
            .OrderBy(a => a.Name == "ticket" ? 1 : 0) // "ticket" last
            .ThenBy(a => a.Name)                      // everything else alphabetical
            .ToList();

            if (IsResponse||IsRequest) {
                propertyNameValuePairs.Add(new DataMemberFieldNameValueAttributes("ticket", Ticketing.TICKET, new Attribute[0], null, typeof(ulong)));
            }
            foreach (DataMemberFieldNameValueAttributes dataMemberPropertyNameValuePair in propertyNameValuePairs)
            {
                if (dataMemberPropertyNameValuePair.TypeFromActualUse == null) continue;
                if (dataMemberPropertyNameValuePair.DataMemberNamesIgnoreAttribute != null) {
                    var ignoreAttribute = dataMemberPropertyNameValuePair.DataMemberNamesIgnoreAttribute;
                    if ((!IsRequest) || (IsRequest && ignoreAttribute.ToJSON)) {
                        if ((!IsResponse) || (IsResponse && ignoreAttribute.FromJSON))
                        {
                            continue;
                        }
                    }
                }
                string lowerCamelCase = StringHelper.LowerCamelCase(dataMemberPropertyNameValuePair.Name);
                DataMemberNamesIgnoreAttribute dataMemberNamesIgnoreAttribute =
                    dataMemberPropertyNameValuePair.DataMemberNamesIgnoreAttribute;
                bool isArray = dataMemberPropertyNameValuePair.TypeFromActualUse.IsArray;
                CPlusPlusType cPlusPlusType = CPlusPlusHelper.TranslateTypeForCPlusPlus(
                !isArray 
                ?dataMemberPropertyNameValuePair.TypeFromActualUse
                :GetArrayEntryType(dataMemberPropertyNameValuePair.TypeFromActualUse));

                DataMemberNamesClassAttribute dataMemberNamesClassAttribute = dataMemberPropertyNameValuePair.DataMemberNamesClassAttribute;
                if (dataMemberNamesClassAttribute == null&& cPlusPlusType.IsClass())
                {
                    Console.WriteLine($"Warning, skipped because had no {nameof(DataMemberNamesClassAttribute)} for property {dataMemberPropertyNameValuePair.Name} for type {dataMemberPropertyNameValuePair.TypeFromActualUse.Name} in class {ClassName}\". This should probably be taken out sooner or later with proper cleaning up. ");
                    continue;
                }
                if (isArray) continue;
                if (cPlusPlusType.IsUnknown()) continue;
                sbPrivateDataMembers.Append("        ");
                sbGetterSignatures.Append("        ");
                string underscoredVariableName = variableNameSource.GetUnderscoredVariableName(
                    dataMemberPropertyNameValuePair, 
                    StringHelper.LowerCamelCase(dataMemberPropertyNameValuePair.Name));
                string variableName = variableNameSource.GetVariableName(
                    dataMemberPropertyNameValuePair,
                    StringHelper.LowerCamelCase(dataMemberPropertyNameValuePair.Name));
                hadParameter = true;
                if (firstVariable)
                {
                    firstVariable = false;
                }
                else
                {
                    sbConstructorSignature.AppendLine(", ");
                    sbConstructor.AppendLine(", ");
                    sbFromJSONSecondPart.Append(", ");
                    sbConstructorPart2.AppendLine(",");
                }
                sbConstructorSignature.Append("           ");
                sbConstructor.Append("    ");
                sbFromJSONSecondPart.Append(variableName);
                if (cPlusPlusType.IsClass())
                {
                    if (dataMemberNamesClassAttribute == null)
                    {
                        throw new Exception($"There was no {nameof(DataMemberNamesClassAttribute)} for property {dataMemberPropertyNameValuePair.Name} for type {dataMemberPropertyNameValuePair.TypeFromActualUse.Name} in class {ClassName}");

                    }
                    DataMemberNamesClass dataMemberNamesClassChild =
                        getDataMemberNamesClass(
                            dataMemberNamesClassAttribute.DataMemberNamesType);
                    if (dataMemberNamesClassChild == null)
                    {
                        throw new Exception($"Could not retrieve {nameof(DataMemberNamesClass)} for property {dataMemberPropertyNameValuePair.Name} in class {_ClassName}");
                    }
                    addInclude(dataMemberNamesClassChild);
                    sbPrivateDataMembers.Append(dataMemberNamesClassChild.ClassName);
                    sbGetterSignatures.Append(dataMemberNamesClassChild.ClassName);
                    sbGetters.Append(dataMemberNamesClassChild.ClassName);
                    sbConstructorSignature.Append(dataMemberNamesClassChild.ClassName);
                    sbConstructor.Append(dataMemberNamesClassChild.ClassName);
                    string jsonVariableName = variableName + "JSON";
                    sbFromJSON.Append("    cJSON* ");
                    sbFromJSON.Append(jsonVariableName);
                    sbFromJSON.Append(" = ");
                    sbFromJSON.Append(GetJHelperFromJSONMethod(cPlusPlusType,
                            dataMemberPropertyNameValuePair.Value));
                    sbFromJSON.Append(";");


                    sbFromJSON.Append("    ");
                    sbFromJSON.Append(dataMemberNamesClassChild.ClassName);
                    sbPrivateDataMembers.Append('*');
                    sbGetterSignatures.Append('*');
                    sbGetters.Append('*');
                    sbConstructorSignature.Append('*');
                    sbConstructor.Append('*');
                    sbFromJSON.Append('*');
                    if (isArray)
                    {
                        sbPrivateDataMembers.Append('*');
                        sbGetterSignatures.Append('*');
                        sbGetters.Append('*');
                        sbConstructorSignature.Append('*');
                        sbConstructor.Append('*');
                        sbFromJSON.Append('*');
                    }
                    sbFromJSON.Append(" ");
                    sbFromJSON.Append(variableName);
                    sbFromJSON.Append(" = ");
                    switch (cPlusPlusType)
                    {
                        case CPlusPlusType.Class:
                            sbFromJSON.Append(ClassName);
                            sbFromJSON.Append("::fromJSON(");
                            sbFromJSON.Append(jsonVariableName);
                            sbFromJSON.Append(")");
                            sbToJSON.AppendLine($"    JHelper::addObject(j, \"{dataMemberPropertyNameValuePair.Value}\", this->{underscoredVariableName}->toJSON());");
                            break;
                        case CPlusPlusType.NullableClass:
                            sbFromJSON.Append(jsonVariableName);
                            sbFromJSON.Append("==nullptr?nullptr:");
                            sbFromJSON.Append(ClassName);
                            sbFromJSON.Append("::fromJSON(");
                            sbFromJSON.Append(jsonVariableName);
                            sbFromJSON.Append(")");
                            sbToJSON.AppendLine($"    JHelper::addNullableObject(j, \"{dataMemberPropertyNameValuePair.Value}\", this->{underscoredVariableName}->toJSON());");
                            break;
                        default:
                            throw new Exception("Not supported");
                    }
                    sbFromJSON.AppendLine(";");
                }
                else
                {
                    sbGetters.Append(cPlusPlusType.GetString());
                    sbPrivateDataMembers.Append(cPlusPlusType.GetString());
                    sbGetterSignatures.Append(cPlusPlusType.GetString());
                    sbConstructorSignature.Append(cPlusPlusType.GetString());
                    sbConstructor.Append(cPlusPlusType.GetString());
                    sbFromJSON.Append("    ");
                    sbFromJSON.Append(cPlusPlusType.GetString());
                    sbFromJSON.Append(" ");
                    sbFromJSON.Append(variableName);
                    sbFromJSON.Append(" = ");
                    sbFromJSON.Append(
                        GetJHelperFromJSONMethod(
                            cPlusPlusType,
                            dataMemberPropertyNameValuePair.Value
                        )
                    );
                    sbFromJSON.AppendLine(";");
                    sbToJSON.Append("    ");
                    sbToJSON.Append(
                    GetJHelperCallToJSON(
                        cPlusPlusType,
                        dataMemberPropertyNameValuePair.Value,
                        underscoredVariableName,
                        null
                        )
                    );
                    sbToJSON.AppendLine(";");
                }
                sbGetters.Append(" ");
                sbGetters.Append(ClassName);
                sbGetters.Append("::");
                sbPrivateDataMembers.Append(" ");
                sbPrivateDataMembers.Append(underscoredVariableName);
                sbPrivateDataMembers.AppendLine(";");
                sbGetterSignatures.Append(" ");
                string getMethodName = "get" + StringHelper.UpperCamelCase(dataMemberPropertyNameValuePair.Name);
                sbGetterSignatures.Append(getMethodName);   
                sbGetterSignatures.AppendLine("() noexcept;");
                sbGetters.Append(getMethodName);
                sbGetters.AppendLine("(){");
                sbGetters.Append("    return this->");
                sbGetters.Append(underscoredVariableName
                    );
                sbGetters.AppendLine(";");
                sbGetters.AppendLine("}");
                sbConstructorSignature.Append(' ');
                sbConstructorSignature.Append(variableName);
                if (IsRequest&&(dataMemberPropertyNameValuePair.Value == Ticketing.TICKET))
                {
                    sbConstructorSignature.Append(" = 0");
                }
                sbConstructor.Append(' ');
                sbConstructor.Append(variableName);
                sbConstructorPart2.Append("        ");
                sbConstructorPart2.Append(underscoredVariableName);
                sbConstructorPart2.Append('(');
                sbConstructorPart2.Append(variableName);
                sbConstructorPart2.Append(')');
            }

            sbConstructorSignature.AppendLine(") noexcept;");
            sbConstructor.Append(")");
            if (hadParameter)
            {
                sbConstructor.Append(":");
                sbImports.AppendLine("#include \"../../JSON/JHelper.hpp\"");
            }
            sbConstructor.AppendLine();
            sbConstructorPart2.AppendLine("{");
            sbConstructorPart2.AppendLine("}");
            sbToJSON.AppendLine($"    JHelper::addString(j, \"{MessageTypes.MessageTypes.Type}\", TYPE);");
            sbToJSON.AppendLine("    return j;");
            sbToJSON.AppendLine("}");
            sbFromJSONSecondPart.AppendLine(");");
            sbFromJSON.Append(sbFromJSONSecondPart);
            sbFromJSON.AppendLine("}");
            sbDeconstructor.AppendLine("}");
            sbHpp.Append(sbIfNDef);
            sbHpp.Append(sbImports);
            sbHpp.Append("class ");
            sbHpp.AppendLine(_ClassName);
            sbHpp.AppendLine("{");
            if (_MessageTypeAttribute != null || IsResponse)
            {
                sbHpp.AppendLine("   public:");
                sbHpp.AppendLine("       static const char* TYPE;");
            }
            else {
                //throw new Exception($"Class {_ClassName} had no {nameof(_MessageTypeAttribute)} and was not a response (woudl be ticketed \"tkd\")");
            }
            sbHpp.Append(sbPrivateDataMembers);
            sbHpp.Append(sbGetterSignatures);
            sbHpp.Append(sbConstructorSignature);
            sbHpp.Append(sbDeconstructorSignature);
            if (IsResponse|| IsMessage)
            {
            }
            if (IsRequest || IsMessage)
            {
            }
            sbHpp.Append(sbFromJSONSignature);
            sbHpp.AppendLine();
            sbHpp.Append(sbToJSONSignature);
            sbHpp.AppendLine();
            sbHpp.AppendLine("};");
            sbHpp.Append("#endif //");
            sbHpp.AppendLine(ifndefName);
            sbCpp.Append("#include \"./");
            sbCpp.Append(ClassName);
            sbCpp.AppendLine(".hpp\"");
            if (_MessageTypeAttribute != null)
            {
                sbCpp.AppendLine($"const char* {ClassName}::TYPE = \"{_MessageTypeAttribute.MessageType}\";");
            }
            else if (IsResponse)
            {
                sbCpp.AppendLine($"const char* {ClassName}::TYPE = \"{TicketedMessageType.Ticketed}\";");
            }
            else {
                    //throw new Exception($"Class {_ClassName} had no {nameof(_MessageTypeAttribute)} and was not a response (woudl be ticketed \"tkd\")");
            }
            sbCpp.Append(sbConstructor);
            sbCpp.Append(sbConstructorPart2);
            sbCpp.Append(sbGetters);
            if (_ClassName.Contains("Request") || _ClassName.Contains("request")) { 
            
            }
            if (IsRequest || IsMessage)
            {
            }
            if (IsResponse || IsMessage)
            {
            }
            sbCpp.Append(sbToJSON);
            sbCpp.Append(sbFromJSON);
            sbCpp.Append(sbDeconstructor);

            hpp = sbHpp.ToString();
            cpp = sbCpp.ToString();
        }
        private static string GetJHelperCallToJSON(
            CPlusPlusType cPlusPlusType,
            string key,
            string underscoredVariableName,
            string? childClassName
            ) {
            return cPlusPlusType switch
            {
                CPlusPlusType.Class => throw new NotImplementedException(),//$"addObject({childClassName}->toJSON(), \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.NullableClass => throw new NotImplementedException(),//$"addNullableObject({childClassName}->toJSON(), \"{key}\", {underscoredVariableName})",
                CPlusPlusType.CharPointer => $"JHelper::addString(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.Int8 => $"JHelper::addInt8(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.UInt8 => $"JHelper::addUInt8(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.Int16 => $"JHelper::addInt16(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.UInt16 => $"JHelper::addUInt16(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.Int32 => $"JHelper::addInt32(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.UInt32 => $"JHelper::addUInt32(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.Int64 => $"JHelper::addInt64(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.UInt64 => $"JHelper::addUInt64(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.Double => $"JHelper::addDouble(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.Bool => $"JHelper::addBool(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.NullableCharPointer => $"JHelper::addNullableString(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.NullableInt8 => $"JHelper::addNullableInt8(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.NullableUInt8 => $"JHelper::addNullableUInt8(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.NullableInt16 => $"JHelper::addNullableInt16(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.NullableUInt16 => $"JHelper::addNullableUInt16(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.NullableInt32 => $"JHelper::addNullableInt32(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.NullableUInt32 => $"JHelper::addNullableUInt32(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.NullableInt64 => $"JHelper::addNullableInt64(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.NullableUInt64 => $"JHelper::addNullableUInt64(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.NullableDouble => $"JHelper::addNullableDouble(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.NullableBool => $"JHelper::addNullableBool(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.Unknown => throw new Exception("Cannot pass unknown")
            };
        }
        private static string GetJHelperFromJSONMethod(
            CPlusPlusType cPlusPlusType,
            string key
        )
        {
            return cPlusPlusType switch
            {
                CPlusPlusType.Class => $"JHelper::getObject(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.NullableClass => $"JHelper::getNullableObject(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.CharPointer => $"JHelper::getString(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.Int8 => $"JHelper::getInt8(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.UInt8 => $"JHelper::getUInt8(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.Int16 => $"JHelper::getInt16(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.UInt16 => $"JHelper::getUInt16(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.Int32 => $"JHelper::getInt32(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.UInt32 => $"JHelper::getUInt32(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.Int64 => $"JHelper::getInt64(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.UInt64 => $"JHelper::getUInt64(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.Double => $"JHelper::getDouble(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.Bool => $"JHelper::getBool(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.NullableCharPointer => $"JHelper::getNullableString(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.NullableInt8 => $"JHelper::getNullableInt8(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.NullableUInt8 => $"JHelper::getNullableUInt8(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.NullableInt16 => $"JHelper::getNullableInt16(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.NullableUInt16 => $"JHelper::getNullableUInt16(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.NullableInt32 => $"JHelper::getNullableInt32(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.NullableUInt32 => $"JHelper::getNullableUInt32(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.NullableInt64 => $"JHelper::getNullableInt64(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.NullableUInt64 => $"JHelper::getNullableUInt64(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.NullableDouble => $"JHelper::getNullableDouble(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.NullableBool => $"JHelper::getNullableBool(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.Unknown=> throw new Exception("Cannot pass unknown")
            };
        }
        public static bool IsSupportedArrayType(Type type)
        {
            // Case 1: Native array
            if (type.IsArray)
                return true;

            // Case 2: Common generic collection types
            if (type.IsGenericType)
            {
                Type genericDef = type.GetGenericTypeDefinition();

                if (genericDef == typeof(List<>) ||
                    genericDef == typeof(HashSet<>) ||
                    genericDef == typeof(IList<>) ||
                    genericDef == typeof(IEnumerable<>) ||
                    genericDef == typeof(ICollection<>))
                {
                    return true;
                }
            }

            return false;
        }

        public static Type GetArrayEntryType(Type type)
        {

            // Case 1: Array type (e.g., int[], string[])
            if (type.IsArray)
            {
                return type.GetElementType();
            }

            // Case 2: Generic types like List<T>, HashSet<T>, IEnumerable<T>
            if (type.IsGenericType)
            {
                Type genericDef = type.GetGenericTypeDefinition();

                // Common 1-arg collection types
                if (genericDef == typeof(List<>) ||
                    genericDef == typeof(HashSet<>) ||
                    genericDef == typeof(IList<>) ||
                    genericDef == typeof(IEnumerable<>) ||
                    genericDef == typeof(ICollection<>))
                {
                    return type.GetGenericArguments()[0];
                }
            }
            throw new Exception($"Could not pass element type for array of type {type.Name}");
        }
        private void ToCPlusPlusClassStrings_GeneratePrivateDataMembersAndProperties(
            StringBuilder sbHpp,
            StringBuilder sbCpp,
            Func<Type, DataMemberNamesClass> getDataMemberNamesClass,
            Action<DataMemberNamesClass> addImport, 
            VariableNameSource<DataMemberFieldNameValueAttributes> variableNameSource) {

        }
        private void ToCPlusPlusClassStrings_GenerateConstructor()
        {

        }
        private void ToCPlusPlusClassStrings_GenerateFromJSON()
        {

        }
        private void ToCPlusPlusClassStrings_GenerateToJSON()
        {

        }
        public string ToJavascriptClassString(Func<Type, DataMemberNamesClass> getDataMemberNamesClass) {
            StringBuilder sb = new StringBuilder();
            List<string> seenImports = new List<string> { _ClassName };
            StringBuilder sbImports = new StringBuilder();
            sb.Append("export default class ");
            sb.AppendLine(_ClassName);
            sb.AppendLine("{");
            Action<DataMemberNamesClass> addImport = (dataMemberNamesClass) => {
                if (seenImports.IndexOf(dataMemberNamesClass.ClassName) >= 0)
                    return;
                sbImports.Append("import ");
                sbImports.Append(dataMemberNamesClass.ClassName);
                sbImports.Append(" from './");
                sbImports.Append(dataMemberNamesClass.ClassName); ;
                sbImports.AppendLine("';");
                seenImports.Add(dataMemberNamesClass.ClassName);
            };
            string[] lowerCamelCasePropertyNames = _DataMemberPropertyNameValuePairs
                .Select(d => StringHelper.LowerCamelCase(d.Name)).ToArray();
            bool declaredN = false;
            Action declareNIfNecessary = () => {
                if (declaredN) return;
                declaredN = true;
                sb.Append("    ");
                sb.AppendLine("const n = (v)=>v!==undefined&&v!==null;");
            };
            if (!IsResponse)
            {
                CreateJavaScriptToJSON(
                    sb,
                    getDataMemberNamesClass,
                    declareNIfNecessary,
                    addImport);
            }

            if (!IsRequest)
            {
                declaredN = false;
                CreateJavaScriptFromJSON(
                    sb,
                    getDataMemberNamesClass,
                    declareNIfNecessary,
                    addImport);
            }
            sb.Append("}");
            return sbImports.ToString() + sb.ToString();
        }
        private void CreateJavaScriptToJSON(
            StringBuilder sb, 
            Func<Type, DataMemberNamesClass> getDataMemberNamesClass,
            Action declareNIfNecessary,
            Action<DataMemberNamesClass> addImport) {

            sb.AppendLine(" static toJSON(o){ ");
            sb.AppendLine("    const r = {};");
            bool first = true;
            if (IsRequest)
            {
                if (_MessageTypeAttribute == null)
                    throw new ArgumentException($"There was no {nameof(MessageTypeAttribute)} for type {_ClassName}");

            }
            if (_MessageTypeAttribute != null)
            {
                sb.Append("   r[\"");
                sb.Append(MessageTypes.MessageTypes.Type);
                sb.Append("\"]=\"");
                if (_MessageTypeAttribute == null)
                    throw new ArgumentException($"There was no {nameof(MessageTypeAttribute)} for type {_ClassName}");
                sb.Append(_MessageTypeAttribute.MessageType);
                sb.AppendLine("\";");
                first = false;
            }
            foreach (DataMemberFieldNameValueAttributes dataMemberPropertyNameValuePair in _DataMemberPropertyNameValuePairs)
            {
                string lowerCamelCase = StringHelper.LowerCamelCase(dataMemberPropertyNameValuePair.Name);
                DataMemberNamesIgnoreAttribute dataMemberNamesIgnoreAttribute = dataMemberPropertyNameValuePair.DataMemberNamesIgnoreAttribute;
                if (dataMemberNamesIgnoreAttribute != null
                    && dataMemberNamesIgnoreAttribute.ToJSON)
                {
                    continue;
                }
                DataMemberNamesClassAttribute dataMemberNamesClassAttribute = dataMemberPropertyNameValuePair.DataMemberNamesClassAttribute;
                if (dataMemberNamesClassAttribute != null)
                {
                    DataMemberNamesClass dataMemberNamesClass = getDataMemberNamesClass(dataMemberNamesClassAttribute.DataMemberNamesType);
                    declareNIfNecessary();
                    sb.Append("    ");
                    sb.Append("if(n(o.");
                    sb.Append(lowerCamelCase);
                    sb.Append("))");
                    sb.AppendLine("");
                    sb.Append("        ");
                    sb.Append("r[\"");
                    sb.Append(dataMemberPropertyNameValuePair.Value);
                    sb.Append("\"]=");

                    if (dataMemberNamesClass == null) throw new Exception($"Had no matching {nameof(DataMemberNamesClass)} for {_ClassName} with name {dataMemberNamesClassAttribute.DataMemberNamesType}");
                    if (!dataMemberNamesClassAttribute.IsArray)
                    {
                        sb.Append(dataMemberNamesClass.ClassName);
                        sb.Append(".toJSON(o.");
                        sb.Append(lowerCamelCase);
                        sb.AppendLine(");");
                    }
                    else
                    {
                        sb.Append("o.");
                        sb.Append(lowerCamelCase);
                        sb.Append(".filter(n).map(a=>");
                        sb.Append(dataMemberNamesClass.ClassName);
                        sb.AppendLine(".toJSON(a));");
                    }
                    addImport(dataMemberNamesClass);
                }
                else
                {
                    sb.Append("");
                    sb.Append("    ");
                    sb.Append("r[\"");
                    sb.Append(dataMemberPropertyNameValuePair.Value);
                    sb.Append("\"]=");
                    sb.Append("o.");
                    sb.Append(lowerCamelCase);
                    sb.AppendLine(";");
                }
            }
            sb.AppendLine("    return r;");
            sb.AppendLine(" }");
        }
        private void CreateJavaScriptFromJSON(
            StringBuilder sb,
            Func<Type, DataMemberNamesClass> getDataMemberNamesClass,
            Action declareNIfNecessary,
            Action<DataMemberNamesClass> addImport)
        {
            sb.AppendLine(" static fromJSON(o){");
            sb.Append("    const r = ");
            sb.AppendLine("{};");
            bool declaredV = false;
            Action declareVIfNecessary = () =>
            {
                if (declaredV) return;
                declaredV = true;
                sb.AppendLine("    let v;");
            };
            foreach (DataMemberFieldNameValueAttributes dataMemberPropertyNameValuePair in _DataMemberPropertyNameValuePairs)
            {
                string lowerCamelCase = StringHelper.LowerCamelCase(dataMemberPropertyNameValuePair.Name);
                DataMemberNamesIgnoreAttribute dataMemberNamesIgnoreAttribute = dataMemberPropertyNameValuePair.DataMemberNamesIgnoreAttribute;
                if (dataMemberNamesIgnoreAttribute != null
                    && dataMemberNamesIgnoreAttribute.FromJSON)
                {
                    continue;
                }
                DataMemberNamesClassAttribute dataMemberNamesClassAttribute = dataMemberPropertyNameValuePair.DataMemberNamesClassAttribute;
                if (dataMemberNamesClassAttribute != null)
                {
                    DataMemberNamesClass dataMemberNamesClass = getDataMemberNamesClass(dataMemberNamesClassAttribute.DataMemberNamesType);
                    if (dataMemberNamesClass == null) throw new Exception($"Had no matching {nameof(DataMemberNamesClass)} for {_ClassName} with name {dataMemberNamesClassAttribute.DataMemberNamesType}");
                    declareNIfNecessary();
                    declareVIfNecessary();
                    sb.Append("    v = o[\"");
                    sb.Append(dataMemberPropertyNameValuePair.Value);
                    sb.AppendLine("\"];");

                    sb.AppendLine("    if(n(v))");
                    sb.Append("        r.");
                    sb.Append(lowerCamelCase);
                    sb.Append("=");
                    if (!dataMemberNamesClassAttribute.IsArray)
                    {
                        sb.Append(dataMemberNamesClass.ClassName);
                        sb.AppendLine(".fromJSON(v)");
                    }
                    else
                    {
                        sb.Append("v.filter(n).map(a=>");
                        sb.Append(dataMemberNamesClass.ClassName);
                        sb.Append(".fromJSON(a");
                        sb.AppendLine("));");
                    }
                    addImport(dataMemberNamesClass);
                }
                else
                {
                    sb.Append("    r.");
                    sb.Append(lowerCamelCase);
                    sb.Append("=");
                    sb.Append("o[\"");
                    sb.Append(dataMemberPropertyNameValuePair.Value);
                    sb.AppendLine("\"];");
                }
            }
            sb.AppendLine("   return r;");
            sb.AppendLine(" }");
        }
    }
}
