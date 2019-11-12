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
		private Boolean drawMore = true;
		private int userSampleRate = 44100;
		private int userBitDepth = 16;
		private WaveformViewer waveformViewerL;
		private WaveformViewer waveformViewerR;
		private FrequencyViewer freqDomain;
		private AudioFile audio;
		private LibLink libLink;
        private WindowingMode windowingMode;

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

        public WindowingMode WindowingMode
        {
            get => windowingMode;
            set => windowingMode = value;
        }

		public ControlBox()
        {
            InitializeComponent();

            windowingMode = WindowingMode.None;
        }

		private void EchoButton_Click(object sender, RoutedEventArgs e)
		{
			if (audio != null)
			{
				audio.makeEcho();
				audio.DeMux();
				waveformViewerL.DrawGraph();
				waveformViewerR.DrawGraph();
				//libLink.setSamples(audio.floatToBytes());
			}
		}

		public byte[] getSamples()
		{
			return audio.floatToBytes();
		}

		private void RecordButton_Click(object sender, RoutedEventArgs e)
		{
			
			if (playing || recording)
			{
				recordButton.Content = "Start Recording";
				libLink.recordStop();
				
				return;
			}
			else
			{
				playing = false;
				recording = true;
				audio = null;
				waveformViewerL.Audio = null;
				waveformViewerR.Audio = null;
				waveformViewerL.DrawGraph();
				waveformViewerR.DrawGraph();
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

		public void FinishedRecording(byte[] samples, int channels, int bitDepth, int sampleRate, Boolean recordingDone, uint change)
		{
			//Boolean result = false;
			if (recordingDone)
			{
				drawMore = true;
			}
			AudioFile recorded = new AudioFile(samples, channels, bitDepth, sampleRate);
			waveformViewerL.Audio = recorded;
			waveformViewerR.Audio = recorded;
			audio = recorded;
			freqDomain.Audio = recorded;
			if (drawMore)
			{
				waveformViewerR.DrawGraph();
				//waveformViewerL.DrawGraph();
				if (waveformViewerL.DrawGraph())
				{
					//result = true;
					drawMore = false;
					
						/*waveformViewerR.DrawGraph();
						waveformViewerL.DrawGraph(); */
				}
			} else
			{
				waveformViewerL.ScrollToNext(change);
				//waveformViewerR.ScrollToNext(change);
			}
			if (recordingDone) { 
				recording = false;
				drawMore = true;
			}
			//return result;
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
			if (audio == null || recording)
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
				audio.floatToBytes();
				libLink.playStart(ref audio.ByteArr, audio.SampleRate,
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

		public void Slider2ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (freqDomain != null)
			{
				freqDomain.NumBins = (int)sl2Value.Value;
			}
		}

		public void Slider3ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (freqDomain != null)
			{
				freqDomain.NumThreads = (int)sl3Value.Value;
			}
		}

		private void DftButton_Click(object sender, RoutedEventArgs e)
		{
			if (audio == null)
			{
				return;
			}
			WaveformViewer useWV = (audio.LeftSelected) ? waveformViewerL : waveformViewerR;
			if (useWV.SelectStart == useWV.SelectEnd)
			{
				return;
			}
			audio.SetSelection(useWV.SelectStart, useWV.SelectEnd, audio.LeftSelected);
            float[] selection = Windowing.Apply(audio.Selection, windowingMode);
			freqDomain.GenerateFromFourier(selection);
			freqDomain.DrawGraph();
		}

		private void filterCBtn_Click(object sender, RoutedEventArgs e)
		{
			if (audio == null)
			{
				return;
			}
			Filter filter = new Filter(freqDomain.NumBins, freqDomain.SelectStart,
				freqDomain.SelectEnd, freqDomain.NumThreads);
			if (audio.Channels == 1)
			{
				audio.Samples = filter.Convolve_Thread(audio.Samples, freqDomain.NumThreads);
				audio.DeMux();
			}
			else
			{
				audio.Left = filter.Convolve_Thread(audio.Left, freqDomain.NumThreads);
				audio.Right = filter.Convolve_Thread(audio.Right, freqDomain.NumThreads);
				audio.Mux();
			}
			
			waveformViewerL.DrawGraph();
			waveformViewerR.DrawGraph();
		}

		private void filterBtn_Click(object sender, RoutedEventArgs e)
		{
			if (audio == null)
			{
				return;
			}
			Filter filter = new Filter(freqDomain.NumBins, freqDomain.SelectStart,
				freqDomain.SelectEnd, freqDomain.NumThreads);
			audio.Left = filter.filter(audio.Left);
			audio.Right = filter.filter (audio.Right);
			audio.Mux();
			waveformViewerL.DrawGraph();
			waveformViewerR.DrawGraph(); 

		}

		private void generateToneBtn_Click(object sender, RoutedEventArgs e)
		{
			if (audio != null)
			{
				//double[] freqs = { 110, 220, 329.63 , 440, 554.37, 659.25, 783.99, 880 };
				//audio.GenerateStabData(freqs);
				double[] freqs = { 2756 };
				audio.GenerateSineData(freqs);
				audio.Mux();
				waveformViewerL.DrawGraph();
				waveformViewerR.DrawGraph();
			}
		}

        private void WindowingSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WindowingMode = (WindowingMode)((sender as ComboBox).SelectedItem);
        }
    }
}
