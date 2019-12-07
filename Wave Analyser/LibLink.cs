using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace Wave_Analyser
{
	//Controls passing recording and playing data to C library code
    public class LibLink
    {
		[DllImport("RecordingLibrary.dll")]
		public static extern void Initialize();
		[DllImport("RecordingLibrary.dll")]
		public static extern void RecordStart(IntPtr hwnd);
		[DllImport("RecordingLibrary.dll")]
		public static extern void RecordStop();
		[DllImport("RecordingLibrary.dll")]
		public static extern void PlayStart(IntPtr hwnd);
		[DllImport("RecordingLibrary.dll")]
		public static extern void PlayStop();
		[DllImport("RecordingLibrary.dll")]
		public static extern IntPtr Record_Proc(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam);
		[DllImport("RecordingLibrary.dll")]
		public static extern void SetPSaveBuffer(IntPtr pSaveBuff);
		[DllImport("RecordingLibrary.dll")]
		public static extern uint GetDWDataLength();
		[DllImport("RecordingLibrary.dll")]
		public static extern void PlayPause(IntPtr hwnd);
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
		public static extern void SetLastPlay(Boolean isLastPlay);
		[DllImport("RecordingLibrary.dll")]
		public static extern void PlayPart();
		[DllImport("RecordingLibrary.dll")]
		public static extern void SetPNewBuffer(IntPtr pSaveBuff, uint length);
		[DllImport("RecordingLibrary.dll")]
		public static extern void ContinuePlay(IntPtr buff, uint length);

		private Boolean playing = false;
		private Boolean recording = false;
		private Boolean recordingDone = false;
		private Boolean isLastPlay = false;
		private IntPtr hwnd;
		private int bufferSize;

		public int BufferSize{ get => bufferSize; set => bufferSize = value; }
		private byte[] samples;
		private int place = 0;
		private uint oldSize = 0;
		private ControlBox controlBox;

		public LibLink(Window mainWin, ControlBox cb)
		{
			controlBox = cb;
			hwnd = new WindowInteropHelper(mainWin).EnsureHandle();
			HwndSource source = HwndSource.FromHwnd(hwnd);
			//SetHandle(hwnd);
			source.AddHook(new HwndSourceHook(WndProc));
			Initialize();
		}

		//Used so C# knows when lib has finished recording or playing
		IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			IntPtr psave = Record_Proc(hwnd, msg, wParam, lParam);
			if (recording && psave != IntPtr.Zero)
			{
				uint size = GetDWDataLength();
				double change = size - oldSize;
				oldSize = size;
				byte[] samplesN = new byte[size];
				if (recordingDone)
				{
					recording = false;
				}
				Marshal.Copy(psave, samplesN, 0, (int)size);
				controlBox.UpdateRecordedData(samplesN,
					Convert.ToInt32(GetChannels()), Convert.ToInt32(GetBitDepth()), Convert.ToInt32(GetSampleRate()), recordingDone, change);	
			}

			if (playing && psave != IntPtr.Zero)
			{
				if (isLastPlay)
				{
					playing = false;
					controlBox.FinishedPlaying();
					controlBox.ScrollForPlay(place);
				} else
				{
					if (place + bufferSize >= samples.Length)
					{
						isLastPlay = true;
						SetLastPlay(true);
					}
					int length = isLastPlay ? samples.Length - place : bufferSize;
					IntPtr psaveN = Marshal.AllocHGlobal(length);
					Marshal.Copy(samples, place, psaveN, length);
					ContinuePlay(psaveN, Convert.ToUInt32(length));
					controlBox.ScrollForPlay(place - length);
					if (!isLastPlay)
					{
						place += length;
					}
				}
			}
			return IntPtr.Zero;
		}

		public void setSamples(byte[] arr) {
			samples = arr;
		}

		public void recordStop()
		{
			recordingDone = true;
			RecordStop();
			
		}

		public void playStart(byte[] samples, double sampleRate, int bitDepth, int channels, int dataLength)
		{
			isLastPlay = false;
			place = 0;
			this.samples = samples;
			if (place + bufferSize > samples.Length)
			{
				isLastPlay = true;
			}
			int length = place + bufferSize > samples.Length ? samples.Length : bufferSize;
			SetBitDepth(Convert.ToUInt16(bitDepth));
			SetChannels(Convert.ToUInt16(channels));
			SetDWDataLength(Convert.ToUInt32(length));
			SetSampleRate(Convert.ToUInt32(sampleRate));
			IntPtr psave1 = Marshal.AllocHGlobal(length);
			Marshal.Copy(samples, place, psave1, length);
			SetPSaveBuffer(psave1);
			place += length;
			playing = true;
			if (!isLastPlay)
			{
				if (place + bufferSize > samples.Length)
				{
					isLastPlay = true;
					
				}
				length = isLastPlay ? samples.Length - place : bufferSize;
				IntPtr psave2 = Marshal.AllocHGlobal(length);
				Marshal.Copy(samples, place, psave2, length);
				SetPNewBuffer(psave2, Convert.ToUInt32(length));
				place += length;
			} else
			{
				SetPNewBuffer(IntPtr.Zero, 0);
			}
			SetLastPlay(isLastPlay);
			PlayStart(hwnd);
			controlBox.ScrollForPlay(place - length);

		}
		public void playStop()
		{
			playing = false;
			PlayStop();
		}

		public void playPause()
		{
			PlayPause(hwnd);
		}

		public void recordStart(int sampleRate, int bitDepth)
		{
			recording = true;
			recordingDone = false;
			Initialize();
			SetBitDepth(Convert.ToUInt16(bitDepth));
			SetChannels(Convert.ToUInt16(1));
			SetSampleRate(Convert.ToUInt32(sampleRate));
			RecordStart(hwnd);
		}

	}
}
