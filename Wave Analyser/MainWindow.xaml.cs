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

namespace Wave_Analyser
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
        public readonly double SAMPLE_RATE = 44100.0;
        public readonly int BIT_DEPTH = 16;

        private WaveformViewer waveformViewer;

		public MainWindow()
		{
			InitializeComponent();
            waveformViewer = new WaveformViewer(SAMPLE_RATE, BIT_DEPTH);
            waveformViewer.GenerateSineData(180, new int[] { 150, 300, 1000 });
            waveformPanel.Children.Add(waveformViewer);
            waveformViewer.DrawGraph();
            frequencyViewer.SetSamples(waveformViewer.GetSamples());
            frequencyViewer.Init();

            compositeButton.Click += CompositeButton_Click;
            sineButton.Click += SineButton_Click;
            randomButton.Click += RandomButton_Click;
            dftButton.Click += DftButton_Click;
        }

        private void RandomButton_Click(object sender, RoutedEventArgs e)
        {
            waveformViewer.GenerateRandomSamples(180);
            waveformViewer.DrawGraph();
        }

        private void SineButton_Click(object sender, RoutedEventArgs e)
        {
            int frequency = int.Parse(frequencyInput.Text);
            waveformViewer.GenerateSineTone(180, frequency);
            waveformViewer.DrawGraph();
        }

        private void CompositeButton_Click(object sender, RoutedEventArgs e)
        {
            waveformViewer.GenerateSineData(180, new int[] { 150, 300, 1000 });
            waveformViewer.DrawGraph();
        }

        private void DftButton_Click(object sender, RoutedEventArgs e)
        {
            frequencyViewer.SetSamples(waveformViewer.GetSamples());
            frequencyViewer.Init();
        }
    }
}
