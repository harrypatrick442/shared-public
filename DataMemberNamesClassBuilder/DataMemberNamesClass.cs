using System;
using System.Linq;
using System.Text;
using Core.Strings;
using MessageTypes.Attributes;
using System.Collections.Generic;
using Core.DataMemberNames;
using BaseMessages.Constants;
using JSON;
using Core.Interfaces;
using DataMemberNamesClassBuilder.Delegates;

namespace DataMemberNamesClassBuilder
{
    public class DataMemberNamesClass
    {
        private string _TypeName;
        public string TypeName { get { return _TypeName; } }
        private string _ClassName;
        public string ClassName { get { return _ClassName; } }
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
            out string hpp, out string cpp, ReservedKey[]? reservedKeys)
        {

            ToCPlusPlusHelper.ToCPlusPlusClassStrings(getDataMemberNamesClass, 
                out hpp, out cpp, ClassName, _DataMemberPropertyNameValuePairs,
                IsRequest, IsResponse, IsMessage, _MessageTypeAttribute,
                reservedKeys: reservedKeys);
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
                sb.Append(MessageTypeDataMemberName.Value);
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
