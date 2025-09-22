using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using Core.NativeExtensions;
namespace Core.Pool
{
	public class FileCachedItems
    {
        private string _DirectoryPath;
        public FileCachedItems(string directoryPath) { 
            _DirectoryPath = directoryPath;
        }
        public FileCachedItem<TObject> GetItem<TObject>(Enum identifier)
        {
            return GetItem<TObject>(identifier.ToInt());
        }
        public FileCachedItem<TObject> GetItem<TObject>(int identifier) {
            return new FileCachedItem<TObject>(identifier, _DirectoryPath);
        }
    }
}
