using System.Collections.Generic;

namespace PluginClipboard
{
	internal class ClipboardHandler
	{
		private static ClipboardHandler _current;

		private readonly List<ClipboardData> _historyList;

		internal static ClipboardHandler Current
		{
			get
			{
				if (_current == null)
				{
					_current = new ClipboardHandler();
				}
				return _current;
			}
		}

		internal ClipboardHandler()
		{
			_historyList = new List<ClipboardData>();
		}

		internal void AddHistoryItem(ClipboardData clipboardData)
		{
			if (clipboardData == null)
			{
				return;
			}

			string itemStr = clipboardData.ToString();
			for (int num = _historyList.Count - 1; num >= 0; num--)
			{
				if (_historyList[num] != null && _historyList[num].ToString() == itemStr)
				{
					_historyList.RemoveAt(num);
				}
			}

			_historyList.Insert(0, clipboardData);
			while (_historyList.Count > 50)
			{
				_historyList.RemoveAt(_historyList.Count - 1);
			}
		}

		internal string GetHistoryItem(int id)
		{
			if (id >= 0 && id < _historyList.Count && _historyList[id] != null)
			{
				return _historyList[id].ToString();
			}
			return string.Empty;
		}

		internal void DeleteHistoryItem(int id)
		{
			if (id >= 0 && id < _historyList.Count)
			{
				_historyList.RemoveAt(id);
			}
		}

		internal void SetHistoryItem(int id)
		{
			if (id >= 0 && id < _historyList.Count && _historyList[id] != null)
			{
				_historyList[id].SetToClipboard();
			}
		}
	}
}
