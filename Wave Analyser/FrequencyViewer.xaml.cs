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
        public static readonly int NUM_Y = 16;
        public static readonly int NUM_X = 16;
        public static readonly int N = 164;
        public static readonly int BAR_WIDTH = 8;

        private AudioFile audio;
        private double[] frequencies;
        private double width;
        private double height;
        private double left;
        private double top;
        private double right;
        private double bottom;
        private double maxFreq;
        private Brush freqChartBrush = (Brush)Application.Current.FindResource("freqChartBrush");

        public FrequencyViewer()
		{
			InitializeComponent();

            NumBins = N;
            maxFreq = 0;

            this.SizeChanged += FrequencyViewer_SizeChanged;
        }

        private void FrequencyViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (audio == null)
                return;

            DrawGraph();
        }

        public AudioFile Audio { set => audio = value; }

        public int NumBins { get; set; }

        public void DrawGraph()
		{
            freqGraph.Children.Clear();
            freqGraph.UpdateLayout();

            MeasureGraph();

            //draw axis
            DrawTools.DrawLine(freqGraph, left, left, top, bottom, freqChartBrush);
            DrawTools.DrawLine(freqGraph, left, right, bottom, bottom, freqChartBrush);

            //draw y axis numbers
            double yGap = height / NUM_Y;

            for (int i = 1; i < NUM_Y + 1; i++)
			{
                double amplitude = maxFreq * ((double) i / (NUM_Y));
                DrawTools.Text(freqGraph, 0, bottom - i * yGap, "" + Math.Round(amplitude, 3), freqChartBrush);
			}

            // draw bars
            double xGap = width / frequencies.Length;
           
            for (int i = 0; i < frequencies.Length; i++)
			{
                DrawTools.DrawBar(freqGraph, left + i * xGap, bottom, BAR_WIDTH, frequencies[i] * (height / maxFreq), freqChartBrush);
            }

            // draw x axis numbers
            xGap = width / NUM_X;

            for (int i = 0; i < NUM_X + 1; i++)
            {
                int frequency = (i * audio.SampleRate / NUM_X);
                DrawTools.Text(freqGraph, left + i * xGap, bottom, frequency + "", freqChartBrush);
            }
		}

		public void GenerateFromFourier(float[] samples)
		{
			Complex[] fourierResults = Fourier.DFT(samples, NumBins);
			frequencies = new double[fourierResults.Length];
            maxFreq = 0;
			for (int i = 0; i < frequencies.Length; i++)
			{
                double amplitude = fourierResults[i].VectorLength();
                frequencies[i] = amplitude;
                if (frequencies[i] > maxFreq)
                    maxFreq = frequencies[i];
			}
            Console.WriteLine("MAX FREQUENCY: " + maxFreq);
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

        private void MeasureGraph()
        {
            width = ActualWidth - (PADDING * 2);
            height = ActualHeight - (PADDING * 2);
            left = PADDING;
            top = PADDING;
            right = left + width;
            bottom = top + height;
        }
	}
}
