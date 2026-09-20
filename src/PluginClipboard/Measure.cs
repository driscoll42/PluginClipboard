using System.Text.RegularExpressions;
using Rainmeter;

namespace PluginClipboard
{
	internal class Measure
	{
		private int _id;

		internal Measure()
		{
			_id = 0;
		}

		internal void Reload(API api, ref double maxValue)
		{
			if (api == null) return;

			// 1. Check explicit Index / Line / Item parameter in .ini
			int indexOpt = api.ReadInt("Index", -1);
			if (indexOpt < 0)
			{
				indexOpt = api.ReadInt("Line", -1);
			}
			if (indexOpt < 0)
			{
				indexOpt = api.ReadInt("Item", -1);
			}

			if (indexOpt >= 0)
			{
				_id = (indexOpt > 0) ? indexOpt - 1 : 0;
				return;
			}

			// 2. Parse measure name (e.g. "MeasureLine1" -> 0, "MeasureLine10" -> 9, "Line5" -> 4)
			string measureName = api.GetMeasureName();
			if (!string.IsNullOrEmpty(measureName))
			{
				Match match = Regex.Match(measureName, @"\d+$");
				if (match.Success)
				{
					int num;
					if (int.TryParse(match.Value, out num) && num > 0)
					{
						_id = num - 1;
					}
				}
			}
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
			if (string.IsNullOrEmpty(args)) return;

			switch (args)
			{
				case "Set":
					ClipboardHandler.Current.SetHistoryItem(_id);
					break;
				case "Delete":
					ClipboardHandler.Current.DeleteHistoryItem(_id);
					break;
				default:
					API.Log(API.LogType.Error, "Unsupported bang: " + args);
					break;
			}
		}
	}
}
