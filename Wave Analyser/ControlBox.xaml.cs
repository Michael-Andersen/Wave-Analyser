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
using Wave_Analyser.Classes;

namespace Wave_Analyser
{
    /// <summary>
    /// Interaction logic for ControlBox.xaml
    /// </summary>
    public partial class ControlBox : UserControl
    {
		private Boolean playing = false;
		private Boolean paused = false;
		private Boolean recording = false;
		private int userSampleRate = 44100;
		private int userBitDepth = 16;

		private WaveformViewer waveformViewerL;
		private WaveformViewer waveformViewerR;
		private FrequencyViewer freqDomain;
		private AudioSignal signal;
		private LibLink libLink;

		public LibLink LibLink { get => libLink; set => libLink = value; }

		public WaveformViewer WaveformViewerL {
			get => waveformViewerL;
			set => waveformViewerL = value;
		}

		public WaveformViewer WaveformViewerR {
			get => waveformViewerR;
			set => waveformViewerR = value;
		}

		public FrequencyViewer FrequencyViewer
		{
			get => freqDomain;
			set => freqDomain = value;
		}

		public ControlBox()
        {
            InitializeComponent();
			signal = new AudioSignal();
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
				libLink.recordStop();
				recordButton.Content = "Start Recording";
				return;
			}
			else
			{
				playing = false;
				recording = true;
				recordButton.Content = "Stop Recording";
				libLink.recordStart(userSampleRate, userBitDepth);
			}
		}

		public void openFile()
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

		public void saveFile()
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			if (saveFileDialog.ShowDialog() == true)
			{
				signal.Mux();
				signal.WriteToFile(saveFileDialog.FileName);
			}
		}

		public void finishedRecording(byte[] samples, int channels, int bitDepth, int sampleRate)
		{
			AudioSignal recorded = new AudioSignal(samples, channels, bitDepth, sampleRate);
			waveformViewerL.Signal = recorded;
			waveformViewerR.Signal = recorded;
			waveformViewerL.DrawGraph();
			waveformViewerR.DrawGraph();
			signal = recorded;
			freqDomain.Signal = recorded;
			recording = false;
		}

		public void finishedPlaying()
		{
			playing = false;
			playButton.Content = "Play";
		}

		private void stopBtn_Click(object sender, RoutedEventArgs e)
		{
			if (playing)
			{
				libLink.playStop();
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
					paused = true;
				}
				else
				{
					playButton.Content = "Pause";
					paused = false;
				}
				libLink.playPause();

			}
			else
			{
				playing = true;
				libLink.playStart(signal.floatToBytes(), signal.SampleRate,
					signal.BitDepth, signal.Channels, signal.Bytes);
				playButton.Content = "Pause";
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
