using BaseMessages.Constants;
using Core.DataMemberNames;
using Core.Strings;
using DataMemberNamesClassBuilder.Delegates.ToCPlusPlus;
using MessageTypes.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataMemberNamesClassBuilder
{
    internal class ToCPlusPlusHelper
    {
        private const string SUCCESS_VARIABLE_NAME = "s";
        public static void ToCPlusPlusClassStrings(Func<Type, DataMemberNamesClass> getDataMemberNamesClass,
            out string hpp, out string cpp, string className,
            DataMemberFieldNameValueAttributes[] dataMemberPropertyNameValuePairs,
            bool isRequest, bool isResponse, bool isMessage, MessageTypeAttribute messageTypeAttribute,
            bool cleanupBucketApproach = true,
            ReservedKey[]? reservedKeys = null)
        {
            if (!cleanupBucketApproach) 
                throw new NotImplementedException($"Not fully implemented yet for {nameof(cleanupBucketApproach)} = true yet...");
            string classNameLower = className.ToLower();
            StringBuilder sbHpp = new StringBuilder();
            StringBuilder sbCpp = new StringBuilder();
            StringBuilder sbImportsHpp = new StringBuilder();
            sbImportsHpp.AppendLine("#include \"../../cJSON/cJSON.h\"");
            sbImportsHpp.AppendLine("#include \"../../JSON/JHelper.hpp\"");
            sbImportsHpp.AppendLine("#include \"../../Core/CleanupBucket.hpp\"");
            sbImportsHpp.AppendLine("#include <memory>");
            var addInclude = Create_AddIncludeOtherMessage(sbImportsHpp, className);
            string[] lowerCamelCasePropertyNames = dataMemberPropertyNameValuePairs
                .Select(d => StringHelper.LowerCamelCase(d.Name)).ToArray();
            string ifndefName = className.ToUpper() + "_HPP";
            StringBuilder sbIfNDef = new StringBuilder();
            sbIfNDef.Append("#ifndef ");
            sbIfNDef.AppendLine(ifndefName);
            sbIfNDef.Append("#define ");
            sbIfNDef.AppendLine(ifndefName);
            sbIfNDef.AppendLine();
            VariableNameSource<DataMemberFieldNameValueAttributes>
                variableNameSource =
                new VariableNameSource<DataMemberFieldNameValueAttributes>();

            StringBuilder sbPrivateDataMembers = new StringBuilder();
            StringBuilder sbGetterSignatures = new StringBuilder();
            StringBuilder sbGetters = new StringBuilder();
            StringBuilder sbSetterSignatures = new StringBuilder();
            StringBuilder sbSetters = new StringBuilder();
            StringBuilder sbConstructorSignature = new StringBuilder();
            StringBuilder sbConstructor = new StringBuilder();
            StringBuilder sbConstructorPart2 = new StringBuilder();
            StringBuilder sbDeconstructorSignature = new StringBuilder();
            StringBuilder sbDeconstructor = new StringBuilder();
            StringBuilder sbDeconstructorPart2 = new StringBuilder();
            StringBuilder sbToJSONSignature = new StringBuilder();
            StringBuilder sbFromJSONSignature = new StringBuilder();
            StringBuilder sbToJSON = new StringBuilder();
            StringBuilder sbFromJSON = new StringBuilder();
            StringBuilder sbFromJSONSecondPart = new StringBuilder();
            sbPrivateDataMembers.AppendLine("   private:");
            sbGetterSignatures.AppendLine("   public:");
            sbConstructorSignature.Append("        ");
            sbConstructorSignature.Append(className);
            sbConstructorSignature.AppendLine("(");
            sbConstructor.Append(className);
            sbConstructor.Append("::");
            sbConstructor.Append(className);
            sbConstructor.AppendLine("(");
            sbDeconstructorSignature.Append("        ~");
            sbDeconstructorSignature.Append(className);
            sbDeconstructorSignature.AppendLine("();");

            sbDeconstructor.Append(className);
            sbDeconstructor.Append("::~");
            sbDeconstructor.Append(className);
            sbDeconstructor.AppendLine("(){");

            sbToJSONSignature.Append("        cJSON* toJSON() noexcept;");
            sbToJSON.Append("cJSON* ");
            sbToJSON.Append(className);
            sbToJSON.AppendLine("::toJSON(){");
            if (cleanupBucketApproach)
            {
                sbFromJSONSignature.Append("        static ");
                sbFromJSONSignature.Append(className);
                sbFromJSONSignature.Append("* fromJSON(cJSON* j, CleanupBucket& cleanupBucket) noexcept;");

                sbFromJSON.Append(className);
                sbFromJSON.Append("* ");
            }
            else
            {
                sbFromJSONSignature.Append("        static std::shared_ptr<");
                sbFromJSONSignature.Append(className);
                sbFromJSONSignature.Append("> fromJSON(cJSON* j) noexcept;");
                sbFromJSON.Append("std::shared_ptr<");
                sbFromJSON.Append(className);
                sbFromJSON.Append("> ");
            }

            sbFromJSON.Append(className);
            sbFromJSON.Append("::fromJSON(cJSON* j");
            if (cleanupBucketApproach)
            {
                sbFromJSON.AppendLine(", CleanupBucket& cleanupBucket){");
            }
            var propertyNameValuePairs = dataMemberPropertyNameValuePairs
            .OrderBy(a => a.Name == "ticket" ? 1 : 0) // "ticket" last
            .ThenBy(a => a.Name)                      // everything else alphabetical
            .ToList();

            if (isResponse || isRequest)
            {
                propertyNameValuePairs.Add(
                    new DataMemberFieldNameValueAttributes(
                    "ticket", Ticketing.TICKET, new Attribute[0], null,
                    typeof(ulong), null));
            }
            if (propertyNameValuePairs.Any())
            {
                sbFromJSON.Append("    bool ");
                sbFromJSON.Append(SUCCESS_VARIABLE_NAME);
                sbFromJSON.AppendLine(" = true;");
            }
            if (cleanupBucketApproach)
            {

                sbFromJSONSecondPart.Append("    auto r = new ");
                sbFromJSONSecondPart.Append(className);
            }
            else
            {
                sbFromJSONSecondPart.Append("    auto r = std::make_shared<");
                sbFromJSONSecondPart.Append(className);
                sbFromJSONSecondPart.Append(">");
            }
            sbFromJSONSecondPart.Append("(");
            sbToJSON.AppendLine("    cJSON *j = cJSON_CreateObject();");
            bool firstVariable = true;
            bool hadParameter = false;
            var checkNotReservedKey = Create_CheckNotReservedKey(reservedKeys);
            var addMemberInitializer = Create_AddMemberInitializer(sbConstructorPart2);
            var addGetterMethod = Create_AddGetterMethod(className, sbGetterSignatures, sbGetters);
            var addSetterMethod = Create_AddSetterMethod(className, sbSetterSignatures, sbSetters);
            var addGetterMethodArray = Create_AddGetterMethodArray(className, sbGetterSignatures, sbGetters);
            var addDeconstructorDeleteArray = Create_AddDeconstructorDeleteArray(
                sbDeconstructorPart2, out Func<bool> getHasVariablesToDeconstruct);
            var toCPlusPlusClassStrings_DataMember_NonClass = Create_DataMember_NonClass(
                cleanupBucketApproach,
                sbPrivateDataMembers, 
                sbConstructor,
                sbConstructorSignature,
                sbFromJSON,
                sbToJSON,
                sbFromJSONSecondPart,
                addMemberInitializer,
                addGetterMethod,
                addSetterMethod,
                addGetterMethodArray,
                addDeconstructorDeleteArray
            );
            var toCPlusPlusClassStrings_DataMember_Class = Create_ToCPlusPlusClassStrings_DataMember_Class(
                cleanupBucketApproach,
                className,
                sbPrivateDataMembers,
                sbConstructor,
                sbConstructorSignature,
                sbFromJSON,
                sbToJSON,
                getDataMemberNamesClass,
                addInclude,
                addMemberInitializer,
                addGetterMethod,
                addDeconstructorDeleteArray
            );
            
            foreach (DataMemberFieldNameValueAttributes dataMemberPropertyNameValuePair
                in propertyNameValuePairs)
            {
                checkNotReservedKey(dataMemberPropertyNameValuePair.Value, dataMemberPropertyNameValuePair.ConsumingType);
                if (dataMemberPropertyNameValuePair.TypeFromActualUse == null) continue;
                if (dataMemberPropertyNameValuePair.DataMemberNamesIgnoreAttribute != null)
                {
                    var ignoreAttribute = dataMemberPropertyNameValuePair.DataMemberNamesIgnoreAttribute;
                    if ((!isRequest) || (isRequest && ignoreAttribute.ToJSON))
                    {
                        if ((!isResponse) || (isResponse && ignoreAttribute.FromJSON))
                        {
                            continue;
                        }
                    }
                }
                string lowerCamelCase = StringHelper.LowerCamelCase(dataMemberPropertyNameValuePair.Name);
                DataMemberNamesIgnoreAttribute dataMemberNamesIgnoreAttribute =
                    dataMemberPropertyNameValuePair.DataMemberNamesIgnoreAttribute;
                bool isArray = dataMemberPropertyNameValuePair.TypeFromActualUse.IsArray;
                CPlusPlusType cPlusPlusType = CPlusPlusHelper
                    .TranslateTypeForCPlusPlus(
                !isArray
                ? dataMemberPropertyNameValuePair.TypeFromActualUse
                : GetArrayEntryType(dataMemberPropertyNameValuePair.TypeFromActualUse),
                dataMemberPropertyNameValuePair.Attributes,
                dataMemberPropertyNameValuePair.ConsumingType,
                dataMemberPropertyNameValuePair.Name);

                DataMemberNamesClassAttribute dataMemberNamesClassAttribute = dataMemberPropertyNameValuePair.DataMemberNamesClassAttribute;
                if (dataMemberNamesClassAttribute == null && cPlusPlusType.IsClass())
                {
                    Console.WriteLine($"Warning, skipped because had no {nameof(DataMemberNamesClassAttribute)} for property {dataMemberPropertyNameValuePair.Name} for type {dataMemberPropertyNameValuePair.TypeFromActualUse.Name} in class {className}\". This should probably be taken out sooner or later with proper cleaning up. ");
                    continue;
                }
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
                }
                sbConstructorSignature.Append("           ");
                sbConstructor.Append("    ");
                sbFromJSONSecondPart.Append(variableName);
                string key = dataMemberPropertyNameValuePair.Value;
                if (cPlusPlusType.IsClass())
                {
                    string propertyClassName = dataMemberPropertyNameValuePair.Name;
                    string propertyClassTypeName = dataMemberPropertyNameValuePair.TypeFromActualUse.Name;
                    toCPlusPlusClassStrings_DataMember_Class(
                        cPlusPlusType,
                        isArray,
                        key,
                        variableName,
                        underscoredVariableName,
                        propertyClassName/*dataMemberPropertyNameValuePair.Name*/,
                        propertyClassTypeName/*dataMemberPropertyNameValuePair.TypeFromActualUse.Name*/,
                        dataMemberNamesClassAttribute
                    );
                }
                else
                {
                    toCPlusPlusClassStrings_DataMember_NonClass(
                        cPlusPlusType,
                         isArray,
                         key,
                         variableName,
                         underscoredVariableName,
                        dataMemberPropertyNameValuePair.ConsumingType,
                        dataMemberPropertyNameValuePair.Name
                    );
                }
                if (isRequest && (dataMemberPropertyNameValuePair.Value == Ticketing.TICKET))
                {
                    sbConstructorSignature.Append(" = 0");
                }
            }
            if (getHasVariablesToDeconstruct())
            {
                if (!firstVariable)
                {
                   // sbConstructorSignature.AppendLine(",");
                    //sbConstructor.AppendLine(",");
                    //sbFromJSONSecondPart.Append(", true");
                }
                //sbConstructorSignature.Append("           bool freeMemoryInDeconstructor");

                //sbConstructor.Append("    bool freeMemoryInDeconstructor");
                addMemberInitializer("_freeMemoryInDeconstructor", "false");
                sbPrivateDataMembers.AppendLine("        bool _freeMemoryInDeconstructor;");

                sbDeconstructor.AppendLine(     "if(!_freeMemoryInDeconstructor)return;");
            }
            sbConstructorSignature.AppendLine(") noexcept;");
            sbConstructor.Append(")");
            if (hadParameter)
            {
                sbConstructor.Append(":");
                sbImportsHpp.AppendLine("#include \"../../JSON/JHelper.hpp\"");
            }
            sbConstructor.AppendLine();
            sbConstructorPart2.AppendLine("{");
            sbConstructorPart2.AppendLine("}");
            sbToJSON.AppendLine($"    JHelper::addString(j, \"{MessageTypeDataMemberName.Value}\", TYPE);");
            sbToJSON.AppendLine("    return j;");
            sbToJSON.AppendLine("}");
            sbFromJSONSecondPart.AppendLine(");");
            if (cleanupBucketApproach)
            {
                sbFromJSONSecondPart.AppendLine("    cleanupBucket.addDelete(r);");
            }
            else
            {
                if (getHasVariablesToDeconstruct())
                {
                    sbFromJSONSecondPart.AppendLine("   r->_freeMemoryInDeconstructor = true;");
                }
            }
            sbFromJSONSecondPart.AppendLine("    return r;");
            sbFromJSONSecondPart.AppendLine("}");
            sbFromJSON.Append(sbFromJSONSecondPart);
            sbDeconstructorPart2.AppendLine("}");
            sbHpp.Append(sbIfNDef);
            sbHpp.Append(sbImportsHpp);
            sbHpp.Append("class ");
            sbHpp.AppendLine(className);
            sbHpp.AppendLine("{");
            if (messageTypeAttribute != null || isResponse)
            {
                sbHpp.AppendLine("   public:");
                sbHpp.AppendLine("       static const char* TYPE;");
            }
            else
            {
                //throw new Exception($"Class {_ClassName} had no {nameof(_MessageTypeAttribute)} and was not a response (woudl be ticketed \"tkd\")");
            }
            sbHpp.Append(sbPrivateDataMembers);
            sbHpp.Append(sbGetterSignatures);
            sbHpp.Append(sbSetterSignatures);
            sbHpp.Append(sbConstructorSignature);
            sbHpp.Append(sbDeconstructorSignature);
            if (isResponse || isMessage)
            {
            }
            if (isRequest || isMessage)
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
            sbCpp.Append(className);
            sbCpp.AppendLine(".hpp\"");
            if (messageTypeAttribute != null)
            {
                sbCpp.AppendLine($"const char* {className}::TYPE = \"{messageTypeAttribute.MessageType}\";");
            }
            else if (isResponse)
            {
                sbCpp.AppendLine($"const char* {className}::TYPE = \"{TicketedMessageType.Ticketed}\";");
            }
            else
            {
                //throw new Exception($"Class {_ClassName} had no {nameof(_MessageTypeAttribute)} and was not a response (woudl be ticketed \"tkd\")");
            }
            sbCpp.Append(sbConstructor);
            sbCpp.Append(sbConstructorPart2);
            sbCpp.Append(sbGetters);
            sbCpp.Append(sbSetters);
            if (className.Contains("Request") || className.Contains("request"))
            {

            }
            if (isRequest || isMessage)
            {
            }
            if (isRequest || isMessage)
            {
            }
            sbCpp.Append(sbToJSON);
            sbCpp.Append(sbFromJSON);
            sbCpp.Append(sbDeconstructor);
            sbCpp.Append(sbDeconstructorPart2);

            hpp = sbHpp.ToString();
            cpp = sbCpp.ToString();
        }
        private static Action<string, Type?> Create_CheckNotReservedKey(ReservedKey[]? reservedKeys) {
            if (reservedKeys == null)
                return (key, actualType) => { };
            var mapKeyTo = new Dictionary<string, ReservedKey>();
            foreach (var reservedKey in reservedKeys) {
                if (mapKeyTo.ContainsKey(reservedKey.Value)) {
                    throw new ArgumentException("Provided the reserve key \"{reservedKey.Value}\" multiple times!");
                }
                mapKeyTo[reservedKey.Value] = reservedKey;
            }
            return (key, actualType) => {
                if (!mapKeyTo.TryGetValue(key, out ReservedKey reservedKey))
                {
                    return;
                }
                if (actualType!=null&&reservedKey.IsAllowedClass(actualType))
                {
                    return;
                }
                throw new Exception($"Key \"{key}\" is reserved ");
            };
        }
        private static Action<string> Create_AddIncludeOtherMessage(
            StringBuilder sbImportsHpp, string className)
        {
            List<string> seenImports = new List<string> { className };
            return (path) => {
                if (seenImports.IndexOf(path) >= 0)
                    return;
                sbImportsHpp.Append("#include \"");
                sbImportsHpp.Append(path); ;
                sbImportsHpp.AppendLine("\"");
                seenImports.Add(path);
            };
        }
        private static DelegateAddDeconstructorDeleteArray 
            Create_AddDeconstructorDeleteArray(
            StringBuilder sbDeconstructorPart2,
            out Func<bool> getHasVariablesToDeconstruct) {
            bool hasVariablesToDeconstruct = false;
            getHasVariablesToDeconstruct = () => hasVariablesToDeconstruct;
            return (string underscoredVariableName) => {
                sbDeconstructorPart2.Append("     if(");
                sbDeconstructorPart2.Append(underscoredVariableName);
                sbDeconstructorPart2.Append("!=nullptr)");
                sbDeconstructorPart2.Append("delete[] ");
                sbDeconstructorPart2.Append(underscoredVariableName);
                sbDeconstructorPart2.AppendLine(";");
                hasVariablesToDeconstruct = true;
            };
        }
        private static Action<string, string> Create_AddMemberInitializer(
            StringBuilder sbConstructorPart2
        )
        {
            bool firstVariable = true;
            return (string underscoredVariableName, string variableName) =>
            {
                if (firstVariable) firstVariable = false;
                else
                    sbConstructorPart2.AppendLine(",");
                sbConstructorPart2.Append("        ");
                sbConstructorPart2.Append(underscoredVariableName);
                sbConstructorPart2.Append('(');
                sbConstructorPart2.Append(variableName);
                sbConstructorPart2.Append(')');
            };
        }
        private static DelegateAddGetterMethod Create_AddGetterMethod(
            string className,
            StringBuilder sbGetterSignatures,
            StringBuilder sbGetters
        )
        {
            return (string typeString, string variableName, string underscoredVariableName) =>
            {
                string getMethodName = "get" + StringHelper.UpperCamelCase(variableName);
                sbGetterSignatures.Append(typeString);
                sbGetterSignatures.Append(" ");
                sbGetterSignatures.Append(getMethodName);
                sbGetterSignatures.AppendLine("()const noexcept;");
                sbGetters.Append(typeString);
                sbGetters.Append(" ");
                sbGetters.Append(className);
                sbGetters.Append("::");
                sbGetters.Append(getMethodName);
                sbGetters.AppendLine("()const noexcept{");
                sbGetters.Append("    return this->");
                sbGetters.Append(underscoredVariableName
                    );
                sbGetters.AppendLine(";");
                sbGetters.AppendLine("}");
            };
        }
        private static DelegateAddSetterMethod Create_AddSetterMethod(
            string className,
            StringBuilder sbSetterSignatures,
            StringBuilder sbSetters
        )
        {
            return (string typeString, string variableName, string underscoredVariableName) =>
            {
                string setMethodName = "set" + StringHelper.UpperCamelCase(variableName);
                sbSetterSignatures.Append("        void ");
                sbSetterSignatures.Append(setMethodName);
                sbSetterSignatures.Append("(");
                sbSetterSignatures.Append(typeString);
                sbSetterSignatures.Append(" value");
                sbSetterSignatures.AppendLine(") noexcept;");
                sbSetters.Append("void ");
                sbSetters.Append(className);
                sbSetters.Append("::");
                sbSetters.Append(setMethodName);
                sbSetters.Append("(");
                sbSetters.Append(typeString);
                sbSetters.AppendLine(" value) noexcept{");
                sbSetters.Append("    this->");
                sbSetters.Append(underscoredVariableName);
                sbSetters.AppendLine(" = value;");
                sbSetters.AppendLine("}");
            };
        }
        private static DelegateAddGetterMethodArray
            Create_AddGetterMethodArray(
                string className,
                StringBuilder sbGetterSignatures,
                StringBuilder sbGetters
            )
        {
            return (string typeString, string variableName,
                string underscoredVariableName, string underscoredLengthVariableName) =>
            {
                string getMethodName = "get" + StringHelper.UpperCamelCase(variableName);
                sbGetterSignatures.Append(typeString);
                sbGetterSignatures.Append(" ");
                sbGetterSignatures.Append(getMethodName);

                sbGetterSignatures.AppendLine("(size_t& length) noexcept;");

                sbGetters.Append(typeString);
                sbGetters.Append(" ");
                sbGetters.Append(className);
                sbGetters.Append("::");
                sbGetters.Append(getMethodName);
                sbGetters.AppendLine("(size_t& length){");
                sbGetters.Append("    length = ");
                sbGetters.Append(underscoredLengthVariableName);
                sbGetters.AppendLine(";");
                sbGetters.Append("    return this->");
                sbGetters.Append(underscoredVariableName);
                sbGetters.AppendLine(";");
                sbGetters.AppendLine("}");
            };
        }
        private static DelegateDataMember_Class
            Create_ToCPlusPlusClassStrings_DataMember_Class(
            bool cleanupBucketApproach,
            string className,
            StringBuilder sbPrivateDataMembers,
            StringBuilder sbConstructor,
            StringBuilder sbConstructorSignature,
            StringBuilder sbFromJSON,
            StringBuilder sbToJSON,
            Func<Type, DataMemberNamesClass> getDataMemberNamesClass,
            Action<string> addInclude,
            Action<string, string> addMemberInitializer,
            DelegateAddGetterMethod addGetterMethod,
            DelegateAddDeconstructorDeleteArray addDeconstructorDeleteArray
            )
        {
            return (
            CPlusPlusType cPlusPlusType,
            bool isArray,
            string key,
            string variableName,
            string underscoredVariableName,
            string propertyClassName/*dataMemberPropertyNameValuePair.Nam*/,
            string propertyClassTypeName/*dataMemberPropertyNameValuePair.TypeFromActualUse.Name*/,
            DataMemberNamesClassAttribute dataMemberNamesClassAttribute) =>
            {

                if (isArray)
                {
                    return;
                }
                if (dataMemberNamesClassAttribute == null)
                {
                    throw new Exception($"There was no {nameof(DataMemberNamesClassAttribute)} for property {propertyClassName} for type {propertyClassTypeName} in class {className}");

                }
                DataMemberNamesClass dataMemberNamesClassChild =
                    getDataMemberNamesClass(
                        dataMemberNamesClassAttribute.DataMemberNamesType);
                if (dataMemberNamesClassChild == null)
                {
                    throw new Exception($"Could not retrieve {nameof(DataMemberNamesClass)} for property {key} in class {className}");
                }
                string fullTypeString;
                if (cleanupBucketApproach)
                {
                    fullTypeString = $"{propertyClassName}*";
                }
                else
                {
                    fullTypeString = $"std::shared_ptr<{propertyClassName}>";
                }
                addInclude($"{dataMemberNamesClassChild.ClassName}.hpp");
                sbPrivateDataMembers.Append(fullTypeString);
                sbConstructorSignature.Append(fullTypeString);
                sbConstructorSignature.Append(" ");
                sbConstructorSignature.Append(variableName);
                sbConstructor.Append(fullTypeString);
                sbConstructor.Append(" ");
                sbConstructor.Append(variableName);
                string jsonVariableName = variableName + "JSON";
                sbFromJSON.Append("    cJSON* ");
                sbFromJSON.Append(jsonVariableName);
                sbFromJSON.Append(" = ");
                sbFromJSON.Append(GetJHelperFromJSONMethod(cPlusPlusType,
                        key));
                sbFromJSON.AppendLine(";");


                sbFromJSON.Append("    ");
                sbFromJSON.Append(fullTypeString);
                /*if (isArray)
                {
                    sbPrivateDataMembers.Append('*');
                    sbGetterSignatures.Append('*');
                    sbGetters.Append('*');
                    sbConstructorSignature.Append('*');
                    sbConstructor.Append('*');
                    sbFromJSON.Append('*');
                }*/
                addGetterMethod(fullTypeString, variableName, underscoredVariableName);
                sbPrivateDataMembers.Append(" ");
                sbPrivateDataMembers.Append(underscoredVariableName);
                sbPrivateDataMembers.AppendLine(";");
                sbFromJSON.Append(" ");
                sbFromJSON.Append(variableName);
                sbFromJSON.Append(" = ");
                switch (cPlusPlusType)
                {
                    case CPlusPlusType.Class:
                        sbFromJSON.Append(propertyClassName);
                        sbFromJSON.Append("::fromJSON(");
                        sbFromJSON.Append(jsonVariableName);
                        if (cleanupBucketApproach) {
                            sbFromJSON.Append(", cleanupBucket");
                        }
                        sbFromJSON.Append(")");
                        addInclude("../../System/Aborter.hpp");
                        sbToJSON.AppendLine($"if(this->{underscoredVariableName}==nullptr) Aborter::safeAbort(\"{className}\",\"{underscoredVariableName} cannot be null\");");
                        sbToJSON.AppendLine($"    JHelper::addObject(j, \"{key}\", this->{underscoredVariableName}->toJSON());");
                        break;
                    case CPlusPlusType.NullableClass:
                        sbFromJSON.Append(jsonVariableName);
                        sbFromJSON.Append("==nullptr?nullptr:");
                        sbFromJSON.Append(propertyClassName);
                        sbFromJSON.Append("::fromJSON(");
                        sbFromJSON.Append(jsonVariableName);
                        if (cleanupBucketApproach)
                        {
                            sbFromJSON.Append(", cleanupBucket");
                        }
                        sbFromJSON.Append(")");
                        addInclude("../../System/Aborter.hpp");
                        sbToJSON.AppendLine($"    JHelper::addNullableObject(j, \"{key}\", this->{underscoredVariableName}==nullptr?nullptr:this->{underscoredVariableName}->toJSON());");
                        break;
                    default:
                        throw new Exception("Not supported");
                }
                sbFromJSON.AppendLine(";");
                addMemberInitializer(underscoredVariableName, variableName);
            };
        }
        private static DelegateDataMember_NonClass
            Create_DataMember_NonClass(
            bool cleanupBucketApproach,
            StringBuilder sbPrivateDataMembers,
            StringBuilder sbConstructor,
            StringBuilder sbConstructorSignature,
            StringBuilder sbFromJSON,
            StringBuilder sbToJSON,
            StringBuilder sbFromJSONSecondPart,
            Action<string, string> addMemberInitializer,
            DelegateAddGetterMethod addGetterMethod,
            DelegateAddSetterMethod addSetterMethod,
            DelegateAddGetterMethodArray addGetterMethodArray,
            DelegateAddDeconstructorDeleteArray addDeconstructorDeleteArray
            )
        {
            return (
            CPlusPlusType cPlusPlusType,
            bool isArray,
            string key,
            string variableName,
            string underscoredVariableName,
            Type classType,
            string propertyNameOnClass) =>
            {
                string typeString = cPlusPlusType.GetString();
                sbPrivateDataMembers.Append(typeString);
                string lengthVariableName = isArray ? $"{variableName}Length" : null;
                string underscoredLengthVariableName = $"_{lengthVariableName}";
                if (isArray)
                {
                    string fullTypeString = typeString + "*";
                    addGetterMethodArray(fullTypeString, variableName, underscoredVariableName,
                        underscoredLengthVariableName);
                }
                else
                {
                    addGetterMethod(typeString, variableName,
                        underscoredVariableName);
                    if (classType != null)
                    {
                        if (HasPublicSetter(classType, propertyNameOnClass))
                        {
                            addSetterMethod(typeString, variableName, underscoredVariableName);
                        }
                    }
                }
                sbConstructor.Append(typeString);
                sbConstructorSignature.Append(typeString);
                if (isArray)
                {
                    sbConstructor.Append("*");
                    sbConstructorSignature.Append("*");
                }
                sbConstructor.Append(' ');
                sbConstructor.Append(variableName);
                sbConstructorSignature.Append(' ');
                sbConstructorSignature.Append(variableName);
                if (isArray)
                {
                    sbFromJSON.Append("    size_t ");
                    sbFromJSON.Append(lengthVariableName);
                    sbFromJSON.AppendLine(";");
                }
                sbFromJSON.Append("    ");
                sbFromJSON.Append(typeString);
                if (isArray)
                {
                    sbPrivateDataMembers.Append("*");
                    sbFromJSON.Append("*");
                    sbConstructor.Append(", size_t");
                    sbConstructor.Append(' ');
                    sbConstructor.Append(lengthVariableName);
                    sbConstructorSignature.AppendLine(",");
                    sbConstructorSignature.Append("           size_t");
                    sbConstructorSignature.Append(' ');
                    sbConstructorSignature.Append(lengthVariableName);
                }
                sbPrivateDataMembers.Append(" ");
                sbPrivateDataMembers.Append(underscoredVariableName);
                sbPrivateDataMembers.AppendLine(";");
                if (isArray)
                {
                    sbPrivateDataMembers.AppendLine($"        size_t {underscoredLengthVariableName};");
                }
                sbFromJSON.Append(" ");
                sbFromJSON.Append(variableName);
                sbFromJSON.Append(" = ");
                sbToJSON.Append("    ");
                if (isArray)
                {
                    sbFromJSON.Append(GetArrayJHelperFromJSONMethod(
                            key: key,
                            lengthVariableName,
                            typeString
                    ));
                    sbToJSON.Append(
                        GetArrayJHelperToJSON(
                            key: key,
                            underscoredVariableName,
                            underscoredLengthVariableName,
                            typeString
                        )
                    );
                }
                else
                {
                    sbFromJSON.Append(
                        GetJHelperFromJSONMethod(
                            cPlusPlusType,
                            key
                        )
                    );
                    sbToJSON.Append(
                        GetJHelperCallToJSON(
                            cPlusPlusType,
                            key,
                            underscoredVariableName,
                            null
                        )
                    );
                }
                sbFromJSON.AppendLine(";");
                sbToJSON.AppendLine(";");
                addMemberInitializer(underscoredVariableName, variableName);
                if (isArray)
                {
                    if (cleanupBucketApproach)
                    {
                        sbFromJSON.Append("    cleanupBucket.addDeleteArray(");
                        sbFromJSON.Append(variableName);
                        sbFromJSON.AppendLine(");");
                    }
                    else {
                        addDeconstructorDeleteArray(underscoredVariableName);
                    }
                    sbFromJSONSecondPart.Append(", ");
                    sbFromJSONSecondPart.Append(lengthVariableName);
                    addMemberInitializer(underscoredLengthVariableName, lengthVariableName);
                }
                else
                {
                    if (cPlusPlusType.Equals(CPlusPlusType.CharPointer))
                    {
                        if (cleanupBucketApproach)
                        {
                            sbFromJSON.Append("    cleanupBucket.addDeleteArray(");
                            sbFromJSON.Append(variableName);
                            sbFromJSON.AppendLine(");");
                        }
                        else
                        {

                            addDeconstructorDeleteArray(underscoredVariableName);
                        }
                    }
                }
            };
        }
        private static string GetArrayJHelperToJSON(
            string key,
            string underscoredVariableName,
            string lengthVariableName,
            string typeString)
        {
            return $"JHelper::addArray<{typeString}>(j, \"{key}\", {underscoredVariableName}, {lengthVariableName})";
        }
        private static string GetJHelperCallToJSON(
            CPlusPlusType cPlusPlusType,
            string key,
            string underscoredVariableName,
            string? childClassName
            )
        {
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
                CPlusPlusType.Float=> $"JHelper::addFloat(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.Double => $"JHelper::addDouble(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.Bool => $"JHelper::addBool(j, \"{key}\", this->{underscoredVariableName})",
                //CPlusPlusType.NullableCharPointer => $"JHelper::addNullableString(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.NullableInt8 => $"JHelper::addNullableInt8(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.NullableUInt8 => $"JHelper::addNullableUInt8(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.NullableInt16 => $"JHelper::addNullableInt16(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.NullableUInt16 => $"JHelper::addNullableUInt16(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.NullableInt32 => $"JHelper::addNullableInt32(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.NullableUInt32 => $"JHelper::addNullableUInt32(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.NullableInt64 => $"JHelper::addNullableInt64(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.NullableUInt64 => $"JHelper::addNullableUInt64(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.NullableFloat => $"JHelper::addNullableFloat(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.NullableDouble => $"JHelper::addNullableDouble(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.NullableBool => $"JHelper::addNullableBool(j, \"{key}\", this->{underscoredVariableName})",
                CPlusPlusType.Unknown => throw new Exception("Cannot pass unknown")
            };
        }
        private static string GetArrayJHelperFromJSONMethod(
            string key,
            string lengthVariableName,
            string typeString)
        {
            return $"JHelper::getArray<{typeString}>(j, \"{key}\", {lengthVariableName})";
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
                CPlusPlusType.Float => $"JHelper::getFloat(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.Double => $"JHelper::getDouble(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.Bool => $"JHelper::getBool(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                //CPlusPlusType.NullableCharPointer => $"JHelper::getNullableString(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.NullableInt8 => $"JHelper::getNullableInt8(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.NullableUInt8 => $"JHelper::getNullableUInt8(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.NullableInt16 => $"JHelper::getNullableInt16(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.NullableUInt16 => $"JHelper::getNullableUInt16(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.NullableInt32 => $"JHelper::getNullableInt32(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.NullableUInt32 => $"JHelper::getNullableUInt32(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.NullableInt64 => $"JHelper::getNullableInt64(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.NullableUInt64 => $"JHelper::getNullableUInt64(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.NullableFloat=> $"JHelper::getNullableFloat(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.NullableDouble => $"JHelper::getNullableDouble(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.NullableBool => $"JHelper::getNullableBool(j, \"{key}\", {SUCCESS_VARIABLE_NAME})",
                CPlusPlusType.Unknown => throw new Exception("Cannot pass unknown")
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
        private static bool HasPublicSetter(Type classType, string propertyNameOnClass) {

            var prop = classType.GetProperty(propertyNameOnClass);
            return prop?.SetMethod != null && prop.SetMethod.IsPublic;
        }
    }
}
