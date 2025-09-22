using Android.Content;
using Core.Exceptions;
using Native.Clipboards;
using System;
using ClipboardManager = Android.Content.ClipboardManager;
namespace NativeAndroid.Clipboards
{
	public sealed class AndroidClipboardWatcher:IClipboardWatcher
	{
		public event EventHandler? ClipboardContentChanged;
		private ClipboardManager? _ClipboardManager;
        private static AndroidClipboardWatcher? _Instance;
		public static AndroidClipboardWatcher Instance
		{
			get
			{
				if (_Instance == null) throw new NotInitializedException(nameof(AndroidClipboardWatcher));
				return _Instance;
			}
		}
		public static AndroidClipboardWatcher Initialize(Context context)
		{
			if (_Instance != null) throw new AlreadyInitializedException(nameof(AndroidClipboardWatcher));
			_Instance = new AndroidClipboardWatcher(context);
			return _Instance;
		}
		public string? GetText()
		{
			return _ClipboardManager?.Text;
		}
		private AndroidClipboardWatcher(Context context)
		{
			_ClipboardManager = context.GetSystemService(Context.ClipboardService) as ClipboardManager;
			if (_ClipboardManager == null) return;
            _ClipboardManager.PrimaryClipChanged += Clipboard_ClipboardContentChanged;

        }

		private void Clipboard_ClipboardContentChanged(object? sender, EventArgs e)
		{
			EventHandler? clipboardContentChanged = ClipboardContentChanged;
			if (clipboardContentChanged == null) return;
			clipboardContentChanged.Invoke(this, new EventArgs());
		}
	}
}