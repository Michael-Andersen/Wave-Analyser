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
        public static readonly int Y_GAP = 50;
        public static readonly int X_GAP = 50;

        private AudioFile audio;
        private double[] frequencies;
        private double width;
        private double height;
        private double left;
        private double top;
        private double right;
        private double bottom;

        private Brush freqChartBrush = (Brush)Application.Current.FindResource("freqChartBrush");

        public FrequencyViewer()
		{
			InitializeComponent();

            this.SizeChanged += FrequencyViewer_SizeChanged;
        }

        private void FrequencyViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (audio == null)
                return;

            DrawGraph(Fourier.BinSize(audio.SampleRate, audio.Selection.Length), audio.NyquistLimit);
        }

        public AudioFile AudioFile { set => audio = value; }

		public void DrawGraph(double binSize, int maxFreq)
		{
            freqGraph.Children.Clear();
            freqGraph.UpdateLayout();
            MeasureGraph();

            //draw axis
            DrawTools.DrawLine(freqGraph, left, left, top, bottom, freqChartBrush);
            DrawTools.DrawLine(freqGraph, left, right, bottom, bottom, freqChartBrush);

            //draw y axis numbers
            for (int i = 0; i < height; i += Y_GAP)
			{
				DrawTools.Text(freqGraph, 0, bottom - i, "" + Math.Round(i * audio.MaxAmp / height, 3), freqChartBrush);
			}

            // draw x axis numbers
            for (int i = 0; i < frequencies.Length; i++)
			{
                DrawTools.DrawBar(freqGraph, left + i * X_GAP, bottom, 8, frequencies[i] * (height / audio.MaxAmp), freqChartBrush);
                DrawTools.Text(freqGraph, left + i * X_GAP, bottom, Math.Round(i * binSize, 0) + "", freqChartBrush);
            }
		}

		public void GenerateFromFourier(float[] samples, int N)
		{
			Complex[] fourierResults = Fourier.DFT(samples, N);
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
