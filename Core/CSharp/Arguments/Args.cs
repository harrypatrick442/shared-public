using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Security.Cryptography;
using File = System.IO.File;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Collections;
using JSON;

namespace Core.Arguments
{
    [DataContract]
    public class Args: IArgs//No not implement IEnumerable here as then it wont serialize properly.
    {    
        [JsonPropertyName("mapKeyNoFlagPollToArg")]
        [JsonInclude]
        [DataMember(Name="mapKeyNoFlagPollToArg")]

        private Dictionary<string, Arg> _MapKeyNoFlagNoPollToArg = new Dictionary<string, Arg>();
        public Args(string[] args) {

            Initialize(FromStrings(args));
        }
        public Args(Arg[] args) {
            Initialize(args);
        }
        protected Args() { }
        private void Initialize(Arg[] args) {
            foreach (Arg arg in args) {
                Add(arg);
            }
        }
        private void Add(Arg arg)
        {
            if (_MapKeyNoFlagNoPollToArg.ContainsKey(arg.KeyNoPoll))
            {
                throw new RepeatedArgumentException(arg, _MapKeyNoFlagNoPollToArg[arg.KeyNoPoll]);
            }
            _MapKeyNoFlagNoPollToArg[arg.KeyNoPoll] = arg;
        }
        public bool HasArg(string key)
        {
            string keyNoPoll = RemovePoll(key);
            return _MapKeyNoFlagNoPollToArg.ContainsKey(keyNoPoll);
        }
        public string GetArgString(string key) {
            Arg arg = GetArg(key);
            if (arg == null) return null;
            return arg.RawValue;
        }
        public T GetArgValue<T>(string key) {
            Arg arg = GetArg(key);
            if (arg == null) return default(T);
            return arg.GetValue<T>();
        }
        public Arg NextAfter(Arg arg) {
            Arg[] argsArray = ToArray();
            int index = Array.IndexOf(argsArray, arg);
            if (index >= argsArray.Length - 1) return null;
            return argsArray[index + 1];
        }
        public T ParseAsBase64EncodedJSON<T>(string key) where T:class
        {
            Arg arg = GetArg(key);
            if (arg == null) return default(T);
            return Json.Deserialize<T>(Encoding.Base64.Decode(arg.RawValue));
        }
        public Arg GetArg(string key) {
            string keyNoPoll = RemovePoll(key);
            if (!_MapKeyNoFlagNoPollToArg.ContainsKey(keyNoPoll)) return null;
            return _MapKeyNoFlagNoPollToArg[keyNoPoll];
        }
        private string RemovePoll(string key) {
            return key.Replace("-" , "");
        }
        private static Arg[] FromStrings(string[] args) { 
            return args.Select((arg) =>
            {
                int indexFirstEquals = arg.IndexOf("=");
                string value = null, key;
                if (indexFirstEquals >= 0)
                {
                    key = arg.Substring(0, indexFirstEquals);//Doing it this way so that the argument can potentially contain base64 encoded data without issues with splitting on"="
                    value = arg.Substring(indexFirstEquals + 1, arg.Length - (1 + indexFirstEquals));
                }
                else
                {
                    key = arg;
                }
                return new Arg(key, value);
            }).ToArray();
        }

        public Arg[] ToArray()
        {
            return _MapKeyNoFlagNoPollToArg.Values.ToArray();
        }
        public override string ToString()
        {
            return string.Join(" ", _MapKeyNoFlagNoPollToArg.Values.Select(arg => arg.ToString()).ToArray());
        }
    }
}
