using System;
using System.Runtime.InteropServices;

namespace Rainmeter
{
	public class API
	{
		public enum LogType
		{
			Error = 1,
			Warning,
			Notice,
			Debug
		}

		private enum RmGetType
		{
			MeasureName,
			Skin,
			SettingsFile,
			SkinName,
			SkinWindowHandle
		}

		private IntPtr m_Rm;

		public API(IntPtr rm)
		{
			m_Rm = rm;
		}

		[DllImport("Rainmeter.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr RmReadString(IntPtr rm, string option, string defValue, bool replaceMeasures);

		[DllImport("Rainmeter.dll", CharSet = CharSet.Unicode)]
		private static extern double RmReadFormula(IntPtr rm, string option, double defValue);

		[DllImport("Rainmeter.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr RmReplaceVariables(IntPtr rm, string str);

		[DllImport("Rainmeter.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr RmPathToAbsolute(IntPtr rm, string relativePath);

		[DllImport("Rainmeter.dll", CharSet = CharSet.Unicode, EntryPoint = "RmExecute")]
		public static extern void Execute(IntPtr skin, string command);

		[DllImport("Rainmeter.dll")]
		private static extern IntPtr RmGet(IntPtr rm, RmGetType type);

		[DllImport("Rainmeter.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		private static extern int LSLog(LogType type, string unused, string message);

		public string ReadString(string option, string defValue, bool replaceMeasures = true)
		{
			return Marshal.PtrToStringUni(RmReadString(m_Rm, option, defValue, replaceMeasures));
		}

		public string ReadPath(string option, string defValue)
		{
			return Marshal.PtrToStringUni(RmPathToAbsolute(m_Rm, ReadString(option, defValue)));
		}

		public double ReadDouble(string option, double defValue)
		{
			return RmReadFormula(m_Rm, option, defValue);
		}

		public int ReadInt(string option, int defValue)
		{
			return (int)RmReadFormula(m_Rm, option, defValue);
		}

		public string ReplaceVariables(string str)
		{
			return Marshal.PtrToStringUni(RmReplaceVariables(m_Rm, str));
		}

		public string GetMeasureName()
		{
			return Marshal.PtrToStringUni(RmGet(m_Rm, RmGetType.MeasureName));
		}

		public IntPtr GetSkin()
		{
			return RmGet(m_Rm, RmGetType.Skin);
		}

		public string GetSettingsFile()
		{
			return Marshal.PtrToStringUni(RmGet(m_Rm, RmGetType.SettingsFile));
		}

		public string GetSkinName()
		{
			return Marshal.PtrToStringUni(RmGet(m_Rm, RmGetType.SkinName));
		}

		public IntPtr GetSkinWindow()
		{
			return RmGet(m_Rm, RmGetType.SkinWindowHandle);
		}

		public static void Log(LogType type, string message)
		{
			LSLog(type, null, message);
		}
	}
}
