using System;
using System.Threading;
using System.Windows.Forms;

namespace PluginClipboard
{
	internal class ClipboardViewer : Form
	{
		private const int WM_DRAWCLIPBOARD = 776;
		private const int WM_CHANGECBCHAIN = 781;

		private static ClipboardViewer _mInstance;
		private static IntPtr _clipboardViewer;

		internal static bool IsStarted;

		internal static void Start()
		{
			IsStarted = true;
			Thread thread = new Thread(RunForm);
			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground = true;
			thread.Start();
		}

		internal static void Stop()
		{
			IsStarted = false;
			if (_mInstance != null && _mInstance.IsHandleCreated)
			{
				_mInstance.Invoke(new MethodInvoker(_mInstance.Close));
			}
		}

		private static void RunForm()
		{
			Application.Run(new ClipboardViewer());
		}

		protected override void OnLoad(EventArgs e)
		{
			base.Visible = false;
			base.ShowInTaskbar = false;
			_mInstance = this;
			_clipboardViewer = NativeMethods.SetClipboardViewer(base.Handle);
			base.OnLoad(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				NativeMethods.ChangeClipboardChain(base.Handle, _clipboardViewer);
			}
			base.Dispose(disposing);
		}

		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				case WM_DRAWCLIPBOARD:
					ClipboardData clipboardData = new ClipboardData(Clipboard.GetDataObject());
					ClipboardHandler.Current.AddHistoryItem(clipboardData);
					NativeMethods.SendMessage(_clipboardViewer, m.Msg, m.WParam, m.LParam);
					break;
				case WM_CHANGECBCHAIN:
					if (m.WParam == _clipboardViewer)
					{
						_clipboardViewer = m.LParam;
					}
					else
					{
						NativeMethods.SendMessage(_clipboardViewer, m.Msg, m.WParam, m.LParam);
					}
					break;
				default:
					base.WndProc(ref m);
					break;
			}
		}
	}
}
