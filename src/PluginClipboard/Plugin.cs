using System;
using System.Runtime.InteropServices;
using RGiesecke.DllExport;
using Rainmeter;

namespace PluginClipboard
{
	public static class Plugin
	{
		private static IntPtr StringBuffer = IntPtr.Zero;

		[DllExport("Initialize", CallingConvention = CallingConvention.Cdecl)]
		public static void Initialize(ref IntPtr data, IntPtr rm)
		{
			data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure()));
			if (!ClipboardViewer.IsStarted)
			{
				ClipboardViewer.Start();
			}
		}

		[DllExport("Finalize", CallingConvention = CallingConvention.Cdecl)]
		public static void Finalize(IntPtr data)
		{
			if (ClipboardViewer.IsStarted)
			{
				Measure.Count = 0;
				ClipboardViewer.Stop();
			}
			GCHandle.FromIntPtr(data).Free();
			if (StringBuffer != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(StringBuffer);
				StringBuffer = IntPtr.Zero;
			}
		}

		[DllExport("Reload", CallingConvention = CallingConvention.Cdecl)]
		public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
		{
			Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
			measure.Reload(new API(rm), ref maxValue);
		}

		[DllExport("Update", CallingConvention = CallingConvention.Cdecl)]
		public static double Update(IntPtr data)
		{
			Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
			return measure.Update();
		}

		[DllExport("GetString", CallingConvention = CallingConvention.Cdecl)]
		public static IntPtr GetString(IntPtr data)
		{
			Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
			if (StringBuffer != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(StringBuffer);
				StringBuffer = IntPtr.Zero;
			}
			string text = measure.GetString();
			if (text != null)
			{
				StringBuffer = Marshal.StringToHGlobalUni(text);
			}
			return StringBuffer;
		}

		[DllExport("ExecuteBang", CallingConvention = CallingConvention.Cdecl)]
		public static void ExecuteBang(IntPtr data, IntPtr args)
		{
			Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
			string stringUni = Marshal.PtrToStringUni(args);
			measure.ExecuteBang(stringUni);
		}
	}
}
