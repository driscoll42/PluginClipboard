using System;
using System.Collections.Generic;
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
				(DataObject o) => "[IMG] " + o.GetImage().Size
			},
			{
				DataFormats.FileDrop,
				(DataObject o) => "[FILE] " + Path.GetFileName(o.GetFileDropList()[0])
			},
			{
				DataFormats.Text,
				(DataObject o) => o.GetText()
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
			string[] formats = dataObject.GetFormats();
			foreach (string format in formats)
			{
				string[] buggyFormats = _buggyFormats;
				Predicate<string> match = (string s) => s == format;
				if (!Array.Exists(buggyFormats, match))
				{
					_dataObject.SetData(format, dataObject.GetData(format));
					if (_convertors.ContainsKey(format))
					{
						_text = _convertors[format](_dataObject);
					}
				}
			}
			if (_text == null)
			{
				_text = "[DATA] " + DateTime.Now.ToString("T");
			}
		}

		internal void SetToClipboard()
		{
			Clipboard.SetDataObject(_dataObject);
		}

		public override string ToString()
		{
			return _text;
		}
	}
}
