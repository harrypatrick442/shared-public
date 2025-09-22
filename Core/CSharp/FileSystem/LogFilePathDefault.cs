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
    public class LogFilePathDefault
    {
        public static readonly string Value = Path.Combine(RootDirectory.Value, "var", "log", "snippets.log");
    }
}

