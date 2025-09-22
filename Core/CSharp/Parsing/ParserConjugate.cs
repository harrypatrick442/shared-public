namespace Core.Parsing
{
    public class ParserConjugate<TFromWhenSerializing, TToWhenSerializing>:IParser<TToWhenSerializing>
    {
        private IParser<TFromWhenSerializing> _IParserFirstWhenSerializing;
        private IParser<TFromWhenSerializing, TToWhenSerializing> _IParserSecondWhenSerializing;
        public ParserConjugate(IParser<TFromWhenSerializing> iParserFirstWhenSerializing, IParser<TFromWhenSerializing, TToWhenSerializing> iParserSecondWhenSerializing) {
            _IParserFirstWhenSerializing = iParserFirstWhenSerializing;
            _IParserSecondWhenSerializing = iParserSecondWhenSerializing;
        }

        public TOut2 Deserialize<TOut2>(TToWhenSerializing payload)
        {
            return _IParserFirstWhenSerializing.Deserialize<TOut2>(_IParserSecondWhenSerializing.Deserialize(payload));
        }

        public IParser<TOut> Pipe<TOut>(IParser<TToWhenSerializing, TOut> pipeThrough)
        {
            return new ParserConjugate<TToWhenSerializing, TOut>(this, pipeThrough);
        }

        public TToWhenSerializing Serialize<TInSerialize>(TInSerialize payload)
        {
            return _IParserSecondWhenSerializing.Serialize(_IParserFirstWhenSerializing.Serialize<TInSerialize>(payload));
        }

        public IParser<TInSerialize, TToWhenSerializing> Wrap<TInSerialize>()
        {
            return new ParserInputTypeFixer<TInSerialize, TToWhenSerializing>(this);
        }
    }

    public class ParserConjugate<TFromWhenSerializing, TIntermediate, TToWhenSerializing> : IParser<TFromWhenSerializing, TToWhenSerializing>
    {
        private IParser<TFromWhenSerializing, TIntermediate> _IParserFirstWhenSerializing;
        private IParser<TIntermediate, TToWhenSerializing> _IParserSecondWhenSerializing;
        public ParserConjugate(IParser<TFromWhenSerializing, TIntermediate> iParserFirstWhenSerializing, IParser<TIntermediate, TToWhenSerializing> iParserSecondWhenSerializing)
        {
            _IParserFirstWhenSerializing = iParserFirstWhenSerializing;
            _IParserSecondWhenSerializing = iParserSecondWhenSerializing;
        }

        public TFromWhenSerializing Deserialize(TToWhenSerializing payload)
        {
            return _IParserFirstWhenSerializing.Deserialize(_IParserSecondWhenSerializing.Deserialize(payload));
        }

        public IParser<TFromWhenSerializing, TOut> Pipe<TOut>(IParser<TToWhenSerializing, TOut> pipeThrough)
        {
            return new ParserConjugate<TFromWhenSerializing, TToWhenSerializing, TOut>(this, pipeThrough);
        }

        public TToWhenSerializing Serialize(TFromWhenSerializing payload)
        {
            return _IParserSecondWhenSerializing.Serialize(_IParserFirstWhenSerializing.Serialize(payload));
        }
    }
}
