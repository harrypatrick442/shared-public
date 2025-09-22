using Core.Parsing;
using System;
namespace Core.Parsers
{
    public class StringToBytes : IParser<string, byte[]>
    {
        private System.Text.Encoding _Encoding;
        public StringToBytes(System.Text.Encoding encoding)
        {
            _Encoding = encoding;
        }
        public string Deserialize(byte[] payload)
        {
            return _Encoding.GetString(payload);
        }

        public IParser<string, TOut> Pipe<TOut>(IParser<byte[], TOut> pipeThrough)
        {
            return new ParserConjugate<string, byte[], TOut>(this, pipeThrough);
        }

        public byte[] Serialize(string payload)
        {
            if (String.IsNullOrEmpty(payload)) return new byte[0];
            return _Encoding.GetBytes(payload);
        }
        public static StringToBytes UTF8() {
            return new StringToBytes(System.Text.Encoding.UTF8);
        }
    }
}
