using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Core.Reflection;
using MessageTypes.Attributes;
using System.Collections.Generic;
using Core.FileSystem;
using System.IO;
using System.Web.Services.Description;

namespace DataMemberNamesClassBuilder
{
    public class JavascriptClassBuilderHelper
    {

        public static void Run(string outputDirectory, string namespaceMustContain,
            Type[] typeInEachNamespace)
        {

            DirectoryHelper.DeleteRecursively(outputDirectory, throwOnError: false);
            Directory.CreateDirectory(outputDirectory);
            ClassExportHelper.Find(
                namespaceMustContain,
                typeInEachNamespace,
                out Type[] dataMemberNamesTypes,
                out DataMemberNamesClass[] dataMemberNamesClasses,
                out Func<Type, DataMemberNamesClass> getDataMemberNamesClass);
            string filePathImportsTxt = Path.Combine(outputDirectory, "imports.txt");
            string filePathIndexJs = Path.Combine(outputDirectory, "index.js");
            StringBuilder sbImport = new StringBuilder();
            StringBuilder sbIndexJs = new StringBuilder();
            sbIndexJs.Append("import MessageTypes from './MessageTypes'; ");
            StringBuilder sbIndexJs2 = new StringBuilder();
            bool first = true;
            sbImport.Append("import { ");

            sbIndexJs2.Append("export{ MessageTypes, ");
            foreach (DataMemberNamesClass dataMemberNameClass in dataMemberNamesClasses)
            {
                if (first) first = false;
                else
                {
                    sbImport.Append(",");
                    sbIndexJs2.Append(",");
                }
                string filePath = Path.Combine(outputDirectory, $"{dataMemberNameClass.ClassName}.js");
                 File.WriteAllText(filePath, dataMemberNameClass.ToJavascriptClassString(getDataMemberNamesClass));
                sbImport.Append(dataMemberNameClass.ClassName);
                sbIndexJs.Append("import ");
                sbIndexJs.Append(dataMemberNameClass.ClassName);
                sbIndexJs.Append(" from './");

                sbIndexJs.Append(dataMemberNameClass.ClassName);
                sbIndexJs.Append("';");
                sbIndexJs2.Append(dataMemberNameClass.ClassName);
            }
            sbIndexJs.AppendLine("");
            sbImport.Append("} from '../messages';");
            sbIndexJs2.Append("};");
            File.WriteAllText(filePathImportsTxt, sbImport.ToString());
            File.WriteAllText(filePathIndexJs, sbIndexJs.ToString() + sbIndexJs2.ToString());
        }
    }
}
