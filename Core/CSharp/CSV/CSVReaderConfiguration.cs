using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;
namespace Core.CSV
{
    public class CSVReaderConfiguration
    {
        public static readonly string[] STANDARD_NEW_LINE_SEQUENCES = new string[] {
            "\r\n",
            "\r" ,
            "\n"
        };
        private char _Delimiter;
        public char Delimiter { get { return _Delimiter; } }
        private string _DelimiterString;
        public string DelimiterString { 
            get
            { 
                if (_DelimiterString == null) _DelimiterString = Delimiter.ToString();
                return _DelimiterString;
            } 
        }
        private string[] _NewLineSequences;
        public string[] NewLineSequences { get {
                return _NewLineSequences;
            } 
        }
        private static CSVReaderConfiguration? _Default;
        public static CSVReaderConfiguration Default { get {
                if (_Default == null) _Default = new CSVReaderConfiguration(',', STANDARD_NEW_LINE_SEQUENCES);
                return _Default;
            } 
        }
        public CSVReaderConfiguration(char delimiter, string[] newLineSequences) {
            _Delimiter = delimiter;
            _NewLineSequences = newLineSequences;
        }
    }
}