using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using JSON;
using System.Reflection;

namespace Core.FileSystem
{
    public class RootDirectory
    {
        public static readonly string Value = Path.GetPathRoot(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location));
    }
}

