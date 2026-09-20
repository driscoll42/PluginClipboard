using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PluginClipboard
{
	internal class ClipboardData
	{
		private delegate TResult Func<in T, out TResult>(T arg);

		private readonly DataObject _dataObject;
		private readonly string _text;

		private readonly Dictionary<string, Func<DataObject, string>> _convertors = new Dictionary<string, Func<DataObject, string>>
		{
			{
				DataFormats.Bitmap,
				(DataObject o) =>
				{
					if (o == null)
					{
						return null;
					}

					try
					{
						Image img = o.GetImage();
						if (img != null)
						{
							return "[IMG] " + img.Size;
						}
					}
					catch
					{
						// Catch GDI+ / format decoding errors gracefully
					}

					return null;
				}
			},
			{
				DataFormats.FileDrop,
				(DataObject o) =>
				{
					if (o == null)
					{
						return null;
					}

					try
					{
						var fileList = o.GetFileDropList();
						if (fileList != null && fileList.Count > 0 && !string.IsNullOrEmpty(fileList[0]))
						{
							return "[FILE] " + Path.GetFileName(fileList[0]);
						}
					}
					catch
					{
						// Catch path parsing / file drop decoding errors gracefully
					}

					return null;
				}
			},
			{
				DataFormats.Text,
				(DataObject o) =>
				{
					if (o == null)
					{
						return null;
					}

					try
					{
						return o.GetText();
					}
					catch
					{
						// Catch text decoding errors gracefully
						return null;
					}
				}
			},
			{
				DataFormats.UnicodeText,
				(DataObject o) =>
				{
					if (o == null)
					{
						return null;
					}

					try
					{
						return o.GetText();
					}
					catch
					{
						// Catch text decoding errors gracefully
						return null;
					}
				}
			},
			{
				DataFormats.StringFormat,
				(DataObject o) =>
				{
					if (o == null)
					{
						return null;
					}

					try
					{
						return o.GetText();
					}
					catch
					{
						// Catch text decoding errors gracefully
						return null;
					}
				}
			}
		};

		private readonly string[] _buggyFormats = new string[3]
		{
			DataFormats.EnhancedMetafile,
			DataFormats.MetafilePict,
			DataFormats.Palette
		};

		internal ClipboardData(IDataObject dataObject)
		{
			_dataObject = new DataObject();

			if (dataObject != null)
			{
				try
				{
					string[] formats = dataObject.GetFormats();
					if (formats != null)
					{
						foreach (string format in formats)
						{
							if (string.IsNullOrEmpty(format))
							{
								continue;
							}

							string[] buggyFormats = _buggyFormats;
							Predicate<string> match = (string s) => s == format;
							if (!Array.Exists(buggyFormats, match))
							{
								try
								{
									object data = dataObject.GetData(format);
									if (data != null)
									{
										_dataObject.SetData(format, data);
									}
								}
								catch
								{
									// Some clipboard formats fail when reading raw data; ignore and continue
								}

								if (_convertors.ContainsKey(format))
								{
									try
									{
										string converted = _convertors[format](_dataObject);
										if (!string.IsNullOrEmpty(converted))
										{
											_text = converted;
										}
									}
									catch
									{
										// Fail gracefully if converter throws
									}
								}
							}
						}
					}
				}
				catch
				{
					// Catch any clipboard format enumeration exceptions
				}
			}

			if (_text == null)
			{
				_text = "[DATA] " + DateTime.Now.ToString("T");
			}
		}

		internal void SetToClipboard()
		{
			if (_dataObject != null)
			{
				try
				{
					Clipboard.SetDataObject(_dataObject);
				}
				catch
				{
					// Clipboard may be temporarily locked by another application
				}
			}
		}

		public override string ToString()
		{
			return _text ?? string.Empty;
		}
	}
}
