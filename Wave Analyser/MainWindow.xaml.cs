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

        private WaveformViewer activeWaveform;
        private AudioSignal signal;

		public MainWindow()
		{
			InitializeComponent();

            signal = new AudioSignal(SAMPLE_RATE, BIT_DEPTH, true);
            activeWaveform = new WaveformViewer();

            waveformPanel.Children.Add(activeWaveform);

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            signal.GenerateSineData(1.0, new int[] { 100 });
            activeWaveform.Signal = signal;
            freqDomain.Signal = signal;
            activeWaveform.DrawGraph();
            freqDomain.Init();

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
            
        }
    }
}
