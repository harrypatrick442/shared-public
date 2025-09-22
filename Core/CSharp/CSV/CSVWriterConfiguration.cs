namespace Core.CSV
{
    public class CSVWriterConfiguration
    {
        private const string STANDARD_NEW_LINE_SEQUENCE = "\r\n";
        public char Delimiter { get; }
        public string DelimiterString =>Delimiter.ToString();
        public string NewLineSequence { get; }
        private static CSVWriterConfiguration? _Default;
        public static CSVWriterConfiguration Default { get {
                if (_Default == null) _Default = new CSVWriterConfiguration(',', STANDARD_NEW_LINE_SEQUENCE);
                return _Default;
            } 
        }
        public CSVWriterConfiguration(char delimiter, string newLineSequence) {
            Delimiter = delimiter;
            NewLineSequence = newLineSequence;
        }
    }
}