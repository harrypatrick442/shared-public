namespace Native.Clipboards
{
	public interface IClipboardWatcher
	{
		public event EventHandler? ClipboardContentChanged;
		public string? GetText();

    }
}