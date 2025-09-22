namespace Native.Events
{
	public class NativeRawMessageEventArgs:EventArgs
    {
		public string RawMessage { get; protected set; }
		public char Type { get { return RawMessage[1]; } }
		public string GetNCharactersAfterType(int nCharacters) {
			return RawMessage.Substring(2, nCharacters);
		}

        public NativeRawMessageEventArgs(string rawMessage) {
			RawMessage = rawMessage;
		}
	}
}