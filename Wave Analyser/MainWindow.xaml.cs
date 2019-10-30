using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Wave_Analyser.Classes;


namespace Wave_Analyser
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		[DllImport("RecordingLibrary.dll")]
		public static extern void Initialize();
		[DllImport("RecordingLibrary.dll")]
		public static extern void RecordStart(IntPtr handle);
		[DllImport("RecordingLibrary.dll")]
		public static extern void RecordStop();
		[DllImport("RecordingLibrary.dll")]
		public static extern void PlayStart(IntPtr handle);
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

		private byte[] samples;
		private Boolean playing = false;
		private Boolean paused = false;
		private Boolean recording = false;
		private int userSampleRate = 44100;
		private int userBitDepth = 16;

		private WaveformViewer waveformViewerL;
		private WaveformViewer waveformViewerR;
        private AudioSignal signal;

		public MainWindow()
		{
			InitializeComponent();
			Initialize();
			IntPtr hwnd = new WindowInteropHelper(this).Handle;
			hwnd = (new WindowInteropHelper(this)).EnsureHandle();
			HwndSource source = HwndSource.FromHwnd(hwnd);
			source.AddHook(new HwndSourceHook(WndProc));
		//	signal = new AudioSignal(SAMPLE_RATE, BIT_DEPTH, true);
			waveformViewerL = new WaveformViewer(true);
			waveformViewerR = new WaveformViewer(false);
			waveformViewerL.OtherChannel = waveformViewerR;
			waveformViewerR.OtherChannel = waveformViewerL;
			waveformPanel.Children.Add(waveformViewerL);
			waveformPanel2.Children.Add(waveformViewerR);
        }

		public void setSignal(AudioSignal signal)
		{
			waveformViewerL.Signal = signal;
			waveformViewerR.Signal = signal;
			waveformViewerL.DrawGraph();
			waveformViewerR.DrawGraph();
			this.signal = signal;
			freqDomain.Signal = signal;
		}

		private void OpenFile_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			if (openFileDialog.ShowDialog() == true)
			{
				signal.readFromFile(openFileDialog.FileName);
				waveformViewerL.Signal = signal;
				waveformViewerL.DrawGraph();
				waveformViewerR.Signal = signal;
				waveformViewerR.DrawGraph();
				freqDomain.Signal = signal;
			}
		}

		private void SaveFile_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			if (saveFileDialog.ShowDialog() == true)
			{
				signal.Mux();
				signal.WriteToFile(saveFileDialog.FileName);
			}
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{

			IntPtr psave = Record_Proc(hwnd, msg, wParam, lParam);
			if (recording && psave != IntPtr.Zero)
			{
				uint size = GetDWDataLength();
				samples = new byte[size];
				recording = false;
				Marshal.Copy(psave, samples, 0, (int)size);
				AudioSignal recorded = new AudioSignal(samples,
					Convert.ToInt32(GetChannels()), Convert.ToInt32(GetBitDepth()), Convert.ToInt32(GetSampleRate()));
				this.setSignal(recorded);
			}

			if (playing && psave != IntPtr.Zero)
			{
				playing = false;
				playButton.Content = "Play";
			}
			return IntPtr.Zero;
		}

		private void EchoButton_Click(object sender, RoutedEventArgs e)
		{
			signal.makeEcho();
			signal.DeMux();
			waveformViewerL.DrawGraph();
			waveformViewerR.DrawGraph();
		}

		private void RecordButton_Click(object sender, RoutedEventArgs e)
		{
			if (playing || recording)
			{
				RecordStop();
				recordButton.Content = "Start Recording";
				return;
			}
			else
			{
				Initialize();
				playing = false;
				recording = true;
				recordButton.Content = "Stop Recording";
				SetBitDepth(Convert.ToUInt16(userBitDepth));
				SetChannels(Convert.ToUInt16(1));
				SetSampleRate(Convert.ToUInt32(userSampleRate));
				RecordStart(new System.Windows.Interop.WindowInteropHelper(this).Handle);
			}
		}

		private void stopBtn_Click(object sender, RoutedEventArgs e)
		{
			if (playing)
			{
				PlayStop();
				playing = false;
				playButton.Content = "Play";
			}
		}
		private void PlayButton_Click(object sender, RoutedEventArgs e)
		{
			if (recording)
			{
				return;
			}
			if (playing)
			{
				if (!paused)
				{
					playButton.Content = "Resume";
				}
				else
				{
					playButton.Content = "Pause";
				}
				PlayPause(new System.Windows.Interop.WindowInteropHelper(this).Handle);

			}
			else
			{
				playing = true;
				SetBitDepth(Convert.ToUInt16(signal.BitDepth));
				SetChannels(Convert.ToUInt16(signal.Channels));
				SetDWDataLength(Convert.ToUInt32(signal.Bytes));
				SetSampleRate(Convert.ToUInt32(signal.SampleRate));
				samples = signal.floatToBytes();
				IntPtr psave = Marshal.AllocHGlobal(samples.Length);
				Marshal.Copy(samples, 0, psave, samples.Length);
				SetPSaveBuffer(psave);
				playButton.Content = "Pause";
				PlayStart(new System.Windows.Interop.WindowInteropHelper(this).Handle);
			}
		}


		private void BitDepthComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (bitDepthComboBox.SelectedIndex == 0)
			{
				userBitDepth = 8;
			}
			else
			{
				userBitDepth = 16;
			}
		}

		public void SliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			userSampleRate = (int)slValue.Value;
		}

		private void DftButton_Click(object sender, RoutedEventArgs e)
		{
			WaveformViewer useWV = (signal.LeftSelected) ? waveformViewerL : waveformViewerR;
			signal.SetSelection(useWV.SelectStart, useWV.SelectEnd, signal.LeftSelected);
			freqDomain.GenerateFromFourier(signal.Selection, signal.Selection.Length);
			freqDomain.DrawGraph(Fourier.BinSize(signal.SampleRate, signal.Selection.Length), 7000);
		}
	}


}
