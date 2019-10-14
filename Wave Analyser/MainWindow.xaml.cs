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
        public readonly int SAMPLE_RATE = 44100;
        public readonly int BIT_DEPTH = 16;
        public readonly int BIN_SIZE = 500;
        public readonly int NUM_SAMPLES = 7000;

        private WaveformViewer waveformViewerL;
		private WaveformViewer waveformViewerR;
        private AudioSignal signal;

		public MainWindow()
		{
			InitializeComponent();

            signal = new AudioSignal(SAMPLE_RATE, BIT_DEPTH, true);
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
			WaveformViewer useWV = (signal.LeftSelected) ? waveformViewerL : waveformViewerR;
            signal.SetSelection(useWV.SelectStart, useWV.SelectEnd, signal.LeftSelected);
            freqDomain.GenerateFromFourier(signal.Selection, signal.Selection.Length);
            freqDomain.DrawGraph(Fourier.BinSize(signal.SampleRate, signal.Selection.Length), 7000);
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
	}
}
