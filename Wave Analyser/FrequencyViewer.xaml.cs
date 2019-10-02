using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Wave_Analyser.Classes;

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

        private AudioSignal signal;
        private double[] frequencies;

        private Brush freqChartBrush = (Brush)Application.Current.FindResource("freqChartBrush");

        public FrequencyViewer()
		{
			InitializeComponent();

            this.SizeChanged += FrequencyViewer_SizeChanged;
        }

        private void FrequencyViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (signal == null)
                return;

            graph.Children.Clear();
            graph.UpdateLayout();

            DrawGraph(Fourier.BinSize(signal.SampleRate, 2000), 7000);
        }

        public void Init()
        {
            GenerateFromFourier(signal.Samples, 2000);
            DrawGraph(Fourier.BinSize(signal.SampleRate, 2000), 7000);
        }

        public AudioSignal Signal { set => signal = value; }

        private void DrawBar(Canvas canvas, double frequency, double amplitude, double width)
		{
			double bottom = 200;
			Rectangle bar = new Rectangle();
			bar.Stroke = freqChartBrush;
			bar.Fill = freqChartBrush;
			bar.Width = 8;
			bar.Height = amplitude;
			Canvas.SetLeft(bar, frequency);
			Canvas.SetTop(bar, bottom - amplitude);
			canvas.Children.Add(bar);
		}

		public void DrawGraph(double binSize, int maxFreq)
		{
            double x = PADDING;
            double y = PADDING;
            double width = maxFreq * 30.0 / binSize;
            double height = (int)ActualHeight - PADDING;
            graph.Width = width;

            //draw axis
            DrawTools.DrawLine(graph, x, x, y, height, freqChartBrush);
            DrawTools.DrawLine(graph, x, width, height, height, freqChartBrush);

            //draw y axis numbers
            for (int i = 0; i < height; i += YSPACING)
			{
				DrawTools.Text(graph, 0, height - i - 10, "" + Math.Round(i * signal.MaxAmp / height, 0), freqChartBrush);
			}

            // draw x axis numbers
            for (int i = 0; i < frequencies.Length; i++)
			{
				DrawBar(graph, 50 + (i) * 30, frequencies[i] * (200.0 / signal.MaxAmp), 10);
				DrawTools.Text(graph, 50 + i * 30, 215, Math.Round(i * binSize, 0) + "", freqChartBrush);
			}
		}

        public void DrawAmpText(int y)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Width = PADDING;
            textBlock.HorizontalAlignment = HorizontalAlignment.Right;
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
