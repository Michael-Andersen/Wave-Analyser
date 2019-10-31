using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Wave_Analyser
{
    public class LibLink
    {
		[DllImport("RecordingLibrary.dll")]
		public static extern void Initialize();
		[DllImport("RecordingLibrary.dll")]
		public static extern void RecordStart();
		[DllImport("RecordingLibrary.dll")]
		public static extern void RecordStop();
		[DllImport("RecordingLibrary.dll")]
		public static extern void PlayStart();
		[DllImport("RecordingLibrary.dll")]
		public static extern void PlayStop();
		[DllImport("RecordingLibrary.dll")]
		public static extern IntPtr Record_Proc(int message, IntPtr wParam, IntPtr lParam);
		[DllImport("RecordingLibrary.dll")]
		public static extern void SetPSaveBuffer(IntPtr pSaveBuff);
		[DllImport("RecordingLibrary.dll")]
		public static extern uint GetDWDataLength();
		[DllImport("RecordingLibrary.dll")]
		public static extern void PlayPause();
		[DllImport("RecordingLibrary.dll")]
		public static extern void SetDWDataLength(uint dwdata);
		[DllImport("RecordingLibrary.dll")]
		public static extern void SetChannels(ushort channels);
		[DllImport("RecordingLibrary.dll")]
		public static extern void SetSampleRate(uint rate);
		[DllImport("RecordingLibrary.dll")]
		public static extern void SetBitDepth(ushort depth);
		[DllImport("RecordingLibrary.dll")]
		public static extern ushort GetChannels();
		[DllImport("RecordingLibrary.dll")]
		public static extern uint GetSampleRate();
		[DllImport("RecordingLibrary.dll")]
		public static extern ushort GetBitDepth();
		[DllImport("RecordingLibrary.dll")]
		public static extern ushort SetHandle(IntPtr handle);

		private Boolean playing = false;
		private Boolean recording = false;
		private ControlBox controlBox;


		public LibLink(MainWindow mainWin, ControlBox cb)
		{
			controlBox = cb;
			IntPtr hwnd = new WindowInteropHelper(mainWin).EnsureHandle();
			HwndSource source = HwndSource.FromHwnd(hwnd);
			SetHandle(hwnd);
			source.AddHook(new HwndSourceHook(WndProc));
			Initialize();
		}

		//Used so C# knows when lib has finished recording or playing
		IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			IntPtr psave = Record_Proc(msg, wParam, lParam);
			if (recording && psave != IntPtr.Zero)
			{
				uint size = GetDWDataLength();
				byte[] samples = new byte[size];
				recording = false;
				Marshal.Copy(psave, samples, 0, (int)size);
				controlBox.finishedRecording(samples,
					Convert.ToInt32(GetChannels()), Convert.ToInt32(GetBitDepth()), Convert.ToInt32(GetSampleRate()));
			}

			if (playing && psave != IntPtr.Zero)
			{
				playing = false;
				controlBox.finishedPlaying();
			}
			return IntPtr.Zero;
		}

		public void recordStop()
		{
			RecordStop();
		}

		public void playStart(byte[] samples, double sampleRate, int bitDepth, int channels, int dataLength)
		{
			SetBitDepth(Convert.ToUInt16(bitDepth));
			SetChannels(Convert.ToUInt16(channels));
			SetDWDataLength(Convert.ToUInt32(dataLength));
			SetSampleRate(Convert.ToUInt32(sampleRate));
			IntPtr psave = Marshal.AllocHGlobal(samples.Length);
			Marshal.Copy(samples, 0, psave, samples.Length);
			SetPSaveBuffer(psave);
			playing = true;
			PlayStart();

		}
		public void playStop()
		{
			PlayStop();
		}

		public void playPause()
		{
			PlayPause();
		}

		public void recordStart(int sampleRate, int bitDepth)
		{
			recording = true;
			Initialize();
			SetBitDepth(Convert.ToUInt16(bitDepth));
			SetChannels(Convert.ToUInt16(1));
			SetSampleRate(Convert.ToUInt32(sampleRate));
			RecordStart();
		}

	}
}
