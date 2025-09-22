using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Core.FileSystem
{

    public static class NullDelimitedAppendedJsonStringsFileHelper
    {
        private delegate bool DelegateCharacterStartDetector(long l, byte b);
        private const int DEFAULT_BUFFER_SIZE = 4096;

        public static void Read(string filePath, List<string> jsonStrings, int nEntries,
            out long nextStartIndexFromBeginningExclusive,
            long? nextIndexToReadFromBackwardsExclusive = null)
        {
            using (Stream stream = File.OpenRead(filePath))
            {
                nextStartIndexFromBeginningExclusive = 0;
                CheckStreamValid(stream);
                byte[] buffer = new byte[DEFAULT_BUFFER_SIZE];
                long length = stream.Length;
                int nEntriesRead = 0;
                if (nextIndexToReadFromBackwardsExclusive == null)
                    nextIndexToReadFromBackwardsExclusive = length;
                StringBuilder sb = new StringBuilder();
                while (nextIndexToReadFromBackwardsExclusive > 0)
                {
                    int nBytesToRead = nextIndexToReadFromBackwardsExclusive > DEFAULT_BUFFER_SIZE
                        ? DEFAULT_BUFFER_SIZE : (int)nextIndexToReadFromBackwardsExclusive;

                    long streamPositionReadingFromInclusive = (long)nextIndexToReadFromBackwardsExclusive - nBytesToRead;
                    stream.Position = streamPositionReadingFromInclusive;
                    ReadNBytesIntoBuffer(stream, buffer, nBytesToRead);
                    int[] indicesOfNullTerminatorsDescending = FindIndesOfNextNullTerminatorInBytes(buffer, nBytesToRead);
                    if (indicesOfNullTerminatorsDescending.Count() < 1)
                    {
                        sb.Insert(0, System.Text.Encoding.UTF8.GetString(buffer, 0, nBytesToRead));
                        nextIndexToReadFromBackwardsExclusive -= nBytesToRead;
                        continue;
                    }
                    if (ReadEntriesAfterNullTerminators(buffer, indicesOfNullTerminatorsDescending, nBytesToRead,
                        sb, jsonStrings, nEntries, ref nEntriesRead, indexReadFromBackwardsExclusive: (long)nextIndexToReadFromBackwardsExclusive,
                        ref nextStartIndexFromBeginningExclusive))
                        return;
                    nextIndexToReadFromBackwardsExclusive -= nBytesToRead;
                }
                if (sb.Length > 0)
                {
                    jsonStrings.Add(sb.ToString());
                    nEntriesRead++;
                }
                nextStartIndexFromBeginningExclusive = 0;
            }
        }
        public static long Append(string filePath, string jsonString)
        {
            using (Stream stream = File.OpenWrite(filePath))
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonString);
                long nBytesAppended = 0;
                stream.Position = stream.Length;
                foreach (byte b in bytes)
                {
                    if (b == 0)
                        throw new InvalidDataException($"{nameof(bytes)} cannot contain NULL 0 as this is the delimiter and illegal in JSON anyway.");
                }
                if (stream.Length > 0)
                {
                    //TODO Check the efficiency of this.
                    byte zero = 0;
                    stream.WriteByte(zero);
                    nBytesAppended++;
                }
                stream.Write(bytes);
                nBytesAppended += bytes.Length;
                stream.Flush();
                return nBytesAppended;
            }
        }
        private static bool ReadEntriesAfterNullTerminators(byte[] buffer, int[] indicesOfNullTerminatorsDescending, int nBytesRead,
            StringBuilder sb, List<string> entries, int nEntries, ref int nEntriesRead, long indexReadFromBackwardsExclusive,
            ref long nextStartIndexFromBeginningExclusive)
        {

            int indexToExclusive = nBytesRead;
            int i = 0;
            while (i < indicesOfNullTerminatorsDescending.Length)
            {
                int indexOfNullTerminator = indicesOfNullTerminatorsDescending[i];
                int indexOfFirstCharacterAfterNullTerminator = indexOfNullTerminator + 1;
                if (indexOfFirstCharacterAfterNullTerminator < indexToExclusive)
                {
                    sb.Insert(0, System.Text.Encoding.UTF8.GetString(buffer, indexOfFirstCharacterAfterNullTerminator, indexToExclusive - indexOfFirstCharacterAfterNullTerminator));
                }
                if (sb.Length > 0)
                {
                    entries.Add(sb.ToString());
                    nEntriesRead++;
                    sb.Clear();
                    if (nEntriesRead >= nEntries)
                    {
                        nextStartIndexFromBeginningExclusive = indexReadFromBackwardsExclusive - (nBytesRead - indexOfNullTerminator);
                        return true;
                    }
                }
                indexToExclusive = indexOfNullTerminator;
                i++;
            }
            if (indexToExclusive > 0)
            {
                sb.Insert(0, System.Text.Encoding.UTF8.GetString(buffer, 0, indexToExclusive));
            }
            return false;
        }
        private static int[] FindIndesOfNextNullTerminatorInBytes(byte[] buffer, int nBytesLength)
        {
            int i = nBytesLength - 1;
            List<int> indices = new List<int>();
            while (i >= 0)
            {
                byte b = buffer[i];
                if (b == 0) indices.Add(i);
                i--;
            }
            return indices.ToArray();
        }
        private static void CheckStreamValid(Stream stream)
        {
            if (!stream.CanSeek)
                throw new NotSupportedException("Unable to seek within stream");
            if (!stream.CanRead)
                throw new NotSupportedException("Unable to read within stream");
        }
        private static void ReadNBytesIntoBuffer(Stream input, byte[] buffer, int nBytesToRead)
        {
            int index = 0;
            while (index < nBytesToRead)
            {
                int read = input.Read(buffer, index, nBytesToRead - index);
                if (read == 0)
                {
                    throw new EndOfStreamException
                        (String.Format("End of stream reached with {0} byte{1} left to read.",
                                       nBytesToRead - index,
                                       nBytesToRead - index == 1 ? "s" : ""));
                }
                index += read;
            }
        }
    }
}
