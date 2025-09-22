namespace Core.Parsing
{
	public class ParserInputTypeFixer<TInWhenSerializing, TOutWhenSerializing> : IParser<TInWhenSerializing, TOutWhenSerializing>
	{
		private IParser<TOutWhenSerializing> _Parser;
		public ParserInputTypeFixer(IParser<TOutWhenSerializing> parser)
		{
			_Parser = parser;
		}
		public TOutWhenSerializing Serialize(TInWhenSerializing payload)
		{
			return _Parser.Serialize<TInWhenSerializing>(payload);
		}

		public TInWhenSerializing Deserialize(TOutWhenSerializing payload)
		{
			return _Parser.Deserialize<TInWhenSerializing>(payload);
		}

		public IParser<TInWhenSerializing, TOut> Pipe<TOut>(IParser<TOutWhenSerializing, TOut> pipeThrough)
		{
			return new ParserConjugate<TInWhenSerializing, TOutWhenSerializing, TOut>(this, pipeThrough);
		}
	}
}
