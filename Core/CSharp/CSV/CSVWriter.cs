using System;
using System.Collections.Generic;
using System.IO;
namespace Core.CSV
{
    public class CsvWriter : IDisposable
    {
        private CSVWriterConfiguration _Configuration;
        private StreamWriter _StreamWriter;
        private bool _HasAtLeastOneValueOnThisRow = false;
        public CsvWriter(string filePath, CSVWriterConfiguration? configuration = null) {
            _Configuration = configuration ?? CSVWriterConfiguration.Default;
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            _StreamWriter = new StreamWriter(new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite));
        }
        public void WriteEntryToCurrentLine(string? value) {
            if (_HasAtLeastOneValueOnThisRow) {
                WriteComma();
            }
            value = CSVHelper.EscapeCsvString(value);
            if (value != null)
            {
                _StreamWriter.Write(value);
            }
            _HasAtLeastOneValueOnThisRow = true;
        }
        public void NextLine() {
            _StreamWriter.Write(_Configuration.NewLineSequence);
            _HasAtLeastOneValueOnThisRow = false;
        }
        private void WriteComma() {
            _StreamWriter.Write(_Configuration.Delimiter);
        }
        public void WriteStringArray(IEnumerable<string[]> lines) {
            bool firstLine = true;
            foreach (string[] line in lines) {
                if (firstLine) {
                    firstLine = false;
                }
                else {
                    NextLine();
                }
                WriteLine(line);
            }
        }
        public void WriteLine(IEnumerable<string?> line) {
            foreach (string? entry in line)
            {
                WriteEntryToCurrentLine(entry);
            }
        }
        public void Dispose()
        {
            if (_StreamWriter != null) {
                _StreamWriter.Flush();
                _StreamWriter.Dispose();
            }
            _StreamWriter = null;
        }
        ~CsvWriter() {
            Dispose();
        }
    }
}