using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace Core.Modelling
{
	[DataContract]
	public class PriceRange
	{
		private int _From;
		[JsonPropertyName("from")]
		[JsonInclude]
		[DataMember(Name ="from")]
		public int From { get { return _From; } protected set { _From = value; } }
		private int _To;
		[JsonPropertyName("to")]
		[JsonInclude]
		[DataMember(Name ="to")]
		public int To { get { return _To; } protected set { _To = value; } }
		public PriceRange(int from, int to) {
			_From = from;
			_To = to;
        }
        protected PriceRange() { }
    }
}
