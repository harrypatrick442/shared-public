using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using JSON;

namespace Core.FileSystem
{

    public class AppendedJsonObjectsFileHelper<TObject>
    {
        private bool _ThrowOnFailParseJsonObject;
        private IJsonParser<TObject> _JsonParser;
        public AppendedJsonObjectsFileHelper(IJsonParser<TObject> jsonParser, bool throwOnFailParseJsonObject)
        {
            _ThrowOnFailParseJsonObject = throwOnFailParseJsonObject;
            _JsonParser = jsonParser;
        }
        public TObject[] Read(string filePath, int nEntries,
            out long nextStartIndexFromBeginningExclusive, 
            long? indexToReadFromBackwardsExclusive=null)
        {
            List<string> jsonStrings = new List<string>();
            NullDelimitedAppendedJsonStringsFileHelper.Read(filePath, jsonStrings, nEntries, 
                out nextStartIndexFromBeginningExclusive, indexToReadFromBackwardsExclusive);
            return jsonStrings
                .Select(_DeserializeWithExceptionHandling)
                .Where(obj => obj != null)
                .ToArray();
        }
        public int Read(string filePath, List<TObject> toList, int nEntries,
            out long nextStartIndexFromBeginningExclusive,
            long? indexToReadFromBackwardsExclusive = null)
        {
            List<string> jsonStrings = new List<string>();
            NullDelimitedAppendedJsonStringsFileHelper.Read(filePath, jsonStrings, nEntries,
                out nextStartIndexFromBeginningExclusive, indexToReadFromBackwardsExclusive);
            int nEntriesRead = 0;
            foreach (string jsonString in jsonStrings)
            {
                TObject obj = _DeserializeWithExceptionHandling(jsonString);
                if (obj == null) continue;
                nEntriesRead++;
                toList.Add(obj);
            }
            return nEntriesRead;
        }
        public long Append(string filePath, params TObject[] objs)
        {
            long nBytesAppended = 0;
            foreach (TObject obj in objs) {
                nBytesAppended+=NullDelimitedAppendedJsonStringsFileHelper.Append(filePath, Json.Serialize(obj));
            }
            return nBytesAppended;
        }
        private TObject _DeserializeWithExceptionHandling(string jsonString)
        {
            try
            {
                return Json.Deserialize<TObject>(jsonString);
            }
            catch (Exception ex)
            {
                if (_ThrowOnFailParseJsonObject)
                    throw new Exception($"Failed to parse {jsonString} to {typeof(TObject).Name}", ex);
            }
            return default(TObject);
        }
    }
}

