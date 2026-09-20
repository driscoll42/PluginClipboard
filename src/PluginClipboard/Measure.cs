using Rainmeter;

namespace PluginClipboard
{
	internal class Measure
	{
		private readonly int _id;
		internal static int Count;

		internal Measure()
		{
			_id = Count;
			Count++;
		}

		internal void Reload(API api, ref double maxValue)
		{
		}

		internal double Update()
		{
			return 0.0;
		}

		internal string GetString()
		{
			return ClipboardHandler.Current.GetHistoryItem(_id);
		}

		internal void ExecuteBang(string args)
		{
			switch (args)
			{
				case "Set":
					ClipboardHandler.Current.SetHistoryItem(_id);
					break;
				case "Delete":
					ClipboardHandler.Current.DeleteHistoryItem(_id);
					break;
			}
		}
	}
}
