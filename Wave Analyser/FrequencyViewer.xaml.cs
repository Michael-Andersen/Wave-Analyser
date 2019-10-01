using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Wave_Analyser
{
	/// <summary>
	/// Interaction logic for FrequencyViewer.xaml
	/// </summary>
	public partial class FrequencyViewer : UserControl
	{
        int[] samples;
        double[] frequencies;

		public FrequencyViewer()
		{
			InitializeComponent();
			Measure(new Size(1000, 1000));
			Arrange(new Rect(0, 0, 1000, 1000));
        }

        public void Init()
        {
            GenerateFromFourier(samples, 2000);
            DrawGraph(Fourier.BinSize(WaveformViewer.sampleRate, 2000), 7000);
        }

        public void SetSamples(int[] samples)
        {
            this.samples = samples;
        }

        private void DrawBar(Canvas canvas, double frequency, double amplitude, double width)
		{
			double bottom = 200;
			Rectangle bar = new Rectangle();
			bar.Stroke = (Brush)Application.Current.FindResource("freqDomainBarBrush");
			bar.Fill = (Brush)Application.Current.FindResource("freqDomainBarBrush");
			bar.Width = 8;
			bar.Height = amplitude;
			Canvas.SetLeft(bar, frequency);
			Canvas.SetTop(bar, bottom - amplitude);
			canvas.Children.Add(bar);
		}

		public void DrawGraph(double binSize, int maxFreq)
		{
            frequencyDomainGraph.Children.Clear();
			DrawTools.DrawLine(frequencyDomainGraph, 45, 45, 200, 0, Brushes.Black);
			for (int i = 0; i < 200; i += 20)
			{
				DrawTools.Text(frequencyDomainGraph, 0, 200 - i,"" + Math.Round(i * WaveformViewer.maxAmp / 300.0, 0), Brushes.Black);
			}
			frequencyDomainGraph.Width = maxFreq * 30.0 / binSize;
			for (int i = 0; i < frequencies.Length; i++)
			{
				DrawBar(frequencyDomainGraph, 50 + (i) * 30, frequencies[i] * (200.0/WaveformViewer.maxAmp), 10);
				DrawTools.Text(frequencyDomainGraph, 50 + i * 30, 215, Math.Round(i * binSize, 0) + "", Brushes.Black);
			}
			double z = fViewer.ViewportWidth;
			fViewer.ScrollToEnd();
		}

		public void GenerateFromFourier(int[] samples, int N)
		{
			Complex[] fourierResults = Fourier.dft(samples, N);
			frequencies = new double[fourierResults.Length];
			for (int i = 0; i < frequencies.Length; i++)
			{
				frequencies[i] = fourierResults[i].VectorLength();
			}

		}

		public void GenerateFreqData()
		{
			Random random = new Random();
			frequencies = new double[1000];
			for (int i = 0; i < frequencies.Length; i++)
			{
				int zero = random.Next(0, 3);
				if (zero == 0)
				{
					frequencies[i] = 0;
				}
				else
				{
					frequencies[i] = random.Next(1, 100);
				}
			}
		}
	}
}
