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
		private AudioFile audio;
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
        }

		private void EchoButton_Click(object sender, RoutedEventArgs e)
		{
            audio.makeEcho();
            audio.DeMux();
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

		public void OpenFile()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			if (openFileDialog.ShowDialog() == true)
			{
                audio = AudioFile.ReadFromFile(openFileDialog.FileName);
				waveformViewerL.Audio = audio;
				waveformViewerL.DrawGraph();
				waveformViewerR.Audio = audio;
				waveformViewerR.DrawGraph();
				freqDomain.Audio = audio;
			}
		}

		public void SaveFile()
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			if (saveFileDialog.ShowDialog() == true)
			{
                audio.Mux();
                audio.WriteToFile(saveFileDialog.FileName);
			}
		}

		public void FinishedRecording(byte[] samples, int channels, int bitDepth, int sampleRate)
		{
			AudioFile recorded = new AudioFile(samples, channels, bitDepth, sampleRate);
			waveformViewerL.Audio = recorded;
			waveformViewerR.Audio = recorded;
			waveformViewerL.DrawGraph();
			waveformViewerR.DrawGraph();
			audio = recorded;
			freqDomain.Audio = recorded;
			recording = false;
		}

		public void FinishedPlaying()
		{
			playing = false;
			playButton.Content = "Play";
		}

		private void StopBtn_Click(object sender, RoutedEventArgs e)
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
				libLink.playStart(audio.floatToBytes(), audio.SampleRate,
                    audio.BitDepth, audio.Channels, audio.Bytes);
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
			WaveformViewer useWV = (audio.LeftSelected) ? waveformViewerL : waveformViewerR;
            audio.SetSelection(useWV.SelectStart, useWV.SelectEnd, audio.LeftSelected);
			freqDomain.GenerateFromFourier(audio.Selection);
			freqDomain.DrawGraph();
		}
	}
}
