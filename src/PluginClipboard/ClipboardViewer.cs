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
				try
				{
					_mInstance.Invoke(new MethodInvoker(_mInstance.Close));
				}
				catch
				{
					// Ignore exception on shutdown
				}
			}
		}

		private static void RunForm()
		{
			try
			{
				Application.Run(new ClipboardViewer());
			}
			catch
			{
				// Prevent application termination on unhandled viewer exceptions
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.Visible = false;
			base.ShowInTaskbar = false;
			_mInstance = this;
			try
			{
				_clipboardViewer = NativeMethods.SetClipboardViewer(base.Handle);
			}
			catch
			{
				// Ignore P/Invoke error
			}
			base.OnLoad(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				try
				{
					NativeMethods.ChangeClipboardChain(base.Handle, _clipboardViewer);
				}
				catch
				{
					// Ignore P/Invoke error
				}
			}
			base.Dispose(disposing);
		}

		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				case WM_DRAWCLIPBOARD:
				{
					try
					{
						IDataObject dataObject = Clipboard.GetDataObject();
						if (dataObject != null)
						{
							ClipboardData clipboardData = new ClipboardData(dataObject);
							ClipboardHandler.Current.AddHistoryItem(clipboardData);
						}
					}
					catch
					{
						// Catch any unexpected clipboard access errors (e.g. CLIPBRD_E_CANT_OPEN)
					}

					try
					{
						NativeMethods.SendMessage(_clipboardViewer, m.Msg, m.WParam, m.LParam);
					}
					catch
					{
						// Ignore SendMessage failure
					}
					break;
				}
				case WM_CHANGECBCHAIN:
				{
					if (m.WParam == _clipboardViewer)
					{
						_clipboardViewer = m.LParam;
					}
					else
					{
						try
						{
							NativeMethods.SendMessage(_clipboardViewer, m.Msg, m.WParam, m.LParam);
						}
						catch
						{
							// Ignore SendMessage failure
						}
					}
					break;
				}
				default:
					base.WndProc(ref m);
					break;
			}
		}
	}
}
