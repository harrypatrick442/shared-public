using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;
namespace Core.CSV
{
    public class CSVReader
    {
        private CSVReaderConfiguration _CsvConfiguration;
        public CSVReader(CSVReaderConfiguration csvConfiguration= null)
        {
            _CsvConfiguration = csvConfiguration ?? CSVReaderConfiguration.Default;
        }
        public IEnumerable<string[]> Read(string content)
        {
            return Read(new StringBuffer(content));
        }
        public IEnumerable<string[]> Read(StreamReader streamReader) {
            DoubleBuffer doubleBuffer = new DoubleBuffer(streamReader);
            return Read(doubleBuffer);
        }
        public IEnumerable<string[]> Read(ICSVBuffer buffer)
        {
            bool isEscaped = false;
            string currentColumn = "";
            List<string> currentColumns = new List<string>();
            while (buffer.HasNext)
            {
                char c = buffer.Next;
                if (c == '"')
                {
                    if (buffer.HasNext)
                    {
                        char nextChar = buffer.LookAhead(1);
                        if (nextChar == '"')
                        {
                            currentColumn += '"';
                            buffer.Advance(1);
                            continue;
                        }
                    }
                    isEscaped = !isEscaped;
                    continue;
                }
                if (isEscaped)
                {
                    currentColumn += c;
                    continue;
                }
                bool foundDeliminator = LookAheadForDeliminator(buffer);
                if (foundDeliminator)
                {
                    currentColumns.Add(currentColumn);
                    currentColumn = "";
                    continue;
                }
                bool foundNewLine = LookAheadForNewLineAndAdanceIfFound(buffer);
                if (foundNewLine)
                {
                    currentColumns.Add(currentColumn);
                    currentColumn = "";
                    yield return currentColumns.ToArray();
                    currentColumns.Clear();
                    continue;
                }
                currentColumn += c;
            }
            if (isEscaped) throw new CSVReaderException("The file content was escaped at the end. Incomplete file");
            currentColumns.Add(currentColumn);
            yield return currentColumns.ToArray();
        }
        private bool LookAheadForDeliminator(ICSVBuffer buffer)
        {
            return LookAheadAndAdvanceIfFound(buffer, _CsvConfiguration.DelimiterString);
        }
        private bool LookAheadForNewLineAndAdanceIfFound(ICSVBuffer doubleBuffer)
        {
            foreach (string newLineSequence in _CsvConfiguration.NewLineSequences)
            {
                bool foundNewLineSequence = LookAheadAndAdvanceIfFound(doubleBuffer, newLineSequence);
                if (foundNewLineSequence) return true;
            }
            return false;
        }
        private bool LookAheadAndAdvanceIfFound(ICSVBuffer buffer, string sequence)
        {
            int length = sequence.Length;
            for (int i = 0; i < length; i++)
            {
                if (buffer.LookAhead(i) != sequence[i])
                {
                    return false;
                }
            }
            buffer.Advance(length-1);
            return true;
        }
    }
}