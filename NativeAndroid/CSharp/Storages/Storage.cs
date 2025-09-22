
using Android.Content;
using Core.Exceptions;
using Java.IO;
using Java.Util;
using Logging;
using Native.Interfaces;
using System;
using System.IO;
using System.Text;
using static Android.Provider.ContactsContract;
using File = Java.IO.File;

namespace NativeAndroid.Storages
{
    public sealed class Storage:IStorage
    {
        private const string DIRECTORY_NAME = "storage";
        private static Storage _Instance;
        public static Storage Initialize(Context context) {
            if (_Instance != null) throw new AlreadyInitializedException(nameof(Storage));
            _Instance = new Storage(context);
            return _Instance;
        }
        public static Storage Instance
        {
            get
            {
                if (_Instance == null) throw new NotInitializedException(nameof(Storage));
                return _Instance;
            }
        }
        private Context _Context;
        private Storage(Context context)
        {
            _Context = context;
        }

        public string GetString(string key)
        {
            try
            {

                File directory = new File(_Context.FilesDir, DIRECTORY_NAME);
                File file = new File(directory, key);
                if (!file.Exists())
                {
                    return null;
                }
                using (FileReader fileReader = new FileReader(file))
                {
                    char[] buffer = new char[1024];
                    int charsRead;
                    StringBuilder sb = new StringBuilder();
                    while ((charsRead = fileReader.Read(buffer, 0, 1024)) > 0) {
                        sb.Append(buffer, 0, charsRead);
                    }
                    return sb.ToString();
                }
            }
            catch(Exception ex) {
                Logs.Default.Error(ex);
                return null;
            }
        }

        public void SetString(string key, string value)
        {
            try
            {
                File directory = new File(_Context.FilesDir, DIRECTORY_NAME);
                File file = new File(directory, key);
                if (value == null)
                {
                    if (!directory.Exists())
                    {
                        return;
                    }
                    file.Delete();
                    return;
                }
                if (!directory.Exists())
                {
                    directory.Mkdir();
                }
                using (FileWriter streamWriter = new FileWriter(file))
                {
                    streamWriter.Write(value);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
            }
            catch (Exception ex) {
                Logs.Default.Error(ex);
            }
        }

        public void DeleteAll()
        {
            try
            {
                File directory = new File(_Context.FilesDir, DIRECTORY_NAME);
                foreach (File file in directory.ListFiles())
                {
                    try
                    {
                        file.Delete();
                    }
                    catch { }
                }
            }
            catch { }
        }
    }
}