using System;
using System.Runtime.InteropServices;
using System.Threading;
using RGiesecke.DllExport;
using Rainmeter;

namespace PluginClipboard
{
	public static class Plugin
	{
		private static IntPtr StringBuffer = IntPtr.Zero;
		private static int _activeMeasuresCount = 0;

		[DllExport("Initialize", CallingConvention = CallingConvention.Cdecl)]
		public static void Initialize(ref IntPtr data, IntPtr rm)
		{
			data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure()));
			Interlocked.Increment(ref _activeMeasuresCount);
			if (!ClipboardViewer.IsStarted)
			{
				ClipboardViewer.Start();
			}
		}

		[DllExport("Finalize", CallingConvention = CallingConvention.Cdecl)]
		public static void Finalize(IntPtr data)
		{
			if (data != IntPtr.Zero)
			{
				GCHandle.FromIntPtr(data).Free();
			}

			int count = Interlocked.Decrement(ref _activeMeasuresCount);
			if (count <= 0)
			{
				_activeMeasuresCount = 0;
				if (ClipboardViewer.IsStarted)
				{
					ClipboardViewer.Stop();
				}
			}

			if (StringBuffer != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(StringBuffer);
				StringBuffer = IntPtr.Zero;
			}
		}

		[DllExport("Reload", CallingConvention = CallingConvention.Cdecl)]
		public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
		{
			if (data != IntPtr.Zero)
			{
				Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
				measure.Reload(new API(rm), ref maxValue);
			}
		}

		[DllExport("Update", CallingConvention = CallingConvention.Cdecl)]
		public static double Update(IntPtr data)
		{
			if (data != IntPtr.Zero)
			{
				Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
				return measure.Update();
			}
			return 0.0;
		}

		[DllExport("GetString", CallingConvention = CallingConvention.Cdecl)]
		public static IntPtr GetString(IntPtr data)
		{
			if (StringBuffer != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(StringBuffer);
				StringBuffer = IntPtr.Zero;
			}
			if (data != IntPtr.Zero)
			{
				Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
				string text = measure.GetString();
				if (text != null)
				{
					StringBuffer = Marshal.StringToHGlobalUni(text);
				}
			}
			return StringBuffer;
		}

		[DllExport("ExecuteBang", CallingConvention = CallingConvention.Cdecl)]
		public static void ExecuteBang(IntPtr data, IntPtr args)
		{
			if (data != IntPtr.Zero)
			{
				Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
				string bangArgs = Marshal.PtrToStringUni(args);
				measure.ExecuteBang(bangArgs);
			}
		}
	}
}
