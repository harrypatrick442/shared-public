using System;
using Core.FileSystem;
using System.IO;

namespace DataMemberNamesClassBuilder
{
    public class CPlusPlusClassBuilderHelper
    {

        public static void Run(string outputDirectory, string namespaceMustContain,
            Type[] typeInEachNamespace, ReservedKey[]? reservedKeys)
        {
            DirectoryHelper.DeleteRecursively(outputDirectory, throwOnError: false);
            Directory.CreateDirectory(outputDirectory);
            ClassExportHelper.Find(
                namespaceMustContain, 
                typeInEachNamespace,
                out Type[] dataMemberNamesTypes,
                out DataMemberNamesClass[] toExports,
                out Func<Type, DataMemberNamesClass> getDataMemberNamesClass);
            foreach (DataMemberNamesClass dataMemberNameClass in toExports)
            {
                string hppFilePath = Path.Combine(outputDirectory,
                    $"{dataMemberNameClass.ClassName}.hpp");
                string cppFilePath = Path.Combine(outputDirectory,
                    $"{dataMemberNameClass.ClassName}.cpp");

                dataMemberNameClass.ToCPlusPlusClassStrings(
                getDataMemberNamesClass, out string hpp, out string cpp,
                reservedKeys);
                File.WriteAllText(
                    hppFilePath, hpp
                );
                File.WriteAllText(
                    cppFilePath, cpp
                );

            }
        }
    }
}
