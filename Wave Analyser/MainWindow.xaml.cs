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
        public readonly double SAMPLE_RATE = 44100.0;
        public readonly int BIT_DEPTH = 16;
        public readonly int BIN_SIZE = 500;
        public readonly int NUM_SAMPLES = 7000;

        private WaveformViewer waveformViewer;
        private AudioSignal signal;

		public MainWindow()
		{
			InitializeComponent();

            signal = new AudioSignal(SAMPLE_RATE, BIT_DEPTH, true);
            waveformViewer = new WaveformViewer();

            waveformPanel.Children.Add(waveformViewer);

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            signal.GenerateSineData(1.0, new int[] { 100, 1000, 5000 });
            waveformViewer.Signal = signal;
            freqDomain.Signal = signal;
            waveformViewer.DrawGraph();

            freqDomain.GenerateFromFourier(signal.Samples, BIN_SIZE);
            freqDomain.DrawGraph(Fourier.BinSize(signal.SampleRate, BIN_SIZE), NUM_SAMPLES);

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
            signal.SetSelection(waveformViewer.SelectStart, waveformViewer.SelectEnd);
            freqDomain.GenerateFromFourier(signal.Selection, signal.Selection.Length);
            freqDomain.DrawGraph(Fourier.BinSize(signal.SampleRate, signal.Selection.Length), 7000);
        }
	}
}
