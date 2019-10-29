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
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
        private WaveformViewer waveformViewerL;
		private WaveformViewer waveformViewerR;
        private AudioFile audio;

		public MainWindow()
		{
			InitializeComponent();

			waveformViewerL = new WaveformViewer(true);
			waveformViewerR = new WaveformViewer(false);
			waveformViewerL.OtherChannel = waveformViewerR;
			waveformViewerR.OtherChannel = waveformViewerL;
			waveformPanel.Children.Add(waveformViewerL);
			waveformPanel2.Children.Add(waveformViewerR);
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            compositeButton.Click += CompositeButton_Click;
            sineButton.Click += SineButton_Click;
            randomButton.Click += RandomButton_Click;
            dftButton.Click += DftButton_Click;
        }

        private void RandomButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void SineButton_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void CompositeButton_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void DftButton_Click(object sender, RoutedEventArgs e)
        {
            WaveformViewer useWV = (audio.LeftSelected) ? waveformViewerL : waveformViewerR;
            audio.SetSelection(useWV.SelectStart, useWV.SelectEnd, audio.LeftSelected);
            freqDomain.GenerateFromFourier(audio.Selection);
            Console.WriteLine(audio.SampleRate);
            Console.WriteLine(audio.NyquistLimit);
            freqDomain.DrawGraph();
        }

		private void OpenFile_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			if (openFileDialog.ShowDialog() == true)
			{
                audio = AudioFile.ReadFromFile(openFileDialog.FileName);
				waveformViewerL.AudioFile = audio;
				waveformViewerL.DrawGraph();
				waveformViewerR.AudioFile = audio;
				waveformViewerR.DrawGraph();
				freqDomain.AudioFile = audio;
			}
		}

		private void SaveFile_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			if (saveFileDialog.ShowDialog() == true)
			{
                audio.WriteToFile(saveFileDialog.FileName);
			}
		}
	}
}
