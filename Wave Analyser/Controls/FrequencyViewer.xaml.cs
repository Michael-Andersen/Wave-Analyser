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
        public static readonly int PADDING = 50;
        public static readonly int YSPACING = 20;
        public static readonly int XSPACING = 30;

        private int[] samples;
        private double[] frequencies;

		public FrequencyViewer()
		{
			InitializeComponent();
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
			bar.Stroke = (Brush)Application.Current.FindResource("freqDomainPrimaryBrush");
			bar.Fill = (Brush)Application.Current.FindResource("freqDomainPrimaryBrush");
			bar.Width = 8;
			bar.Height = amplitude;
			Canvas.SetLeft(bar, frequency);
			Canvas.SetTop(bar, bottom - amplitude);
			canvas.Children.Add(bar);
		}

		public void DrawGraph(double binSize, int maxFreq)
		{
            frequencyDomainGraph.Children.Clear();

            double x = PADDING;
            double y = PADDING;
            double width = maxFreq * 30.0 / binSize;
            double height = (int)ActualHeight - PADDING;
            frequencyDomainGraph.Width = width;

            //draw axis
            DrawTools.DrawLine(frequencyDomainGraph, x, x, y, height, 
                (Brush)Application.Current.FindResource("freqDomainPrimaryBrush"));
            DrawTools.DrawLine(frequencyDomainGraph, x, width, height, height, 
                (Brush)Application.Current.FindResource("freqDomainPrimaryBrush"));

            //draw y axis numbers
            for (int i = 0; i < height; i += YSPACING)
			{
				DrawTools.Text(frequencyDomainGraph, 0, height - i - 10, "" + Math.Round(i * WaveformViewer.maxAmp / height, 0), 
                    (Brush)Application.Current.FindResource("freqDomainSecondaryBrush"));
			}

            // draw x axis numbers
            for (int i = 0; i < frequencies.Length; i++)
			{
				DrawBar(frequencyDomainGraph, 50 + (i) * 30, frequencies[i] * (200.0 / WaveformViewer.maxAmp), 10);
				DrawTools.Text(frequencyDomainGraph, 50 + i * 30, 215, Math.Round(i * binSize, 0) + "", Brushes.Black);
			}
		}

        public void DrawAmpText(int y)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Width = PADDING;
            textBlock.HorizontalAlignment = HorizontalAlignment.Right;
            textBlock.Text = 
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
