﻿using System;
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
        public static readonly int Y_GAP = 30;
        public static readonly int X_GAP = 30;

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

            freqGraph.Children.Clear();
            freqGraph.UpdateLayout();

            DrawfreqGraph(Fourier.BinSize(signal.SampleRate, 2000), 7000);
        }

        public void Init()
        {
            GenerateFromFourier(signal.Samples, 2000);
            DrawfreqGraph(Fourier.BinSize(signal.SampleRate, 2000), 7000);
        }

        public AudioSignal Signal { set => signal = value; }

		public void DrawfreqGraph(double binSize, int maxFreq)
		{
            double left = PADDING;
            double top = PADDING;
            double width = maxFreq * X_GAP / binSize;
            double height = (int)ActualHeight - (PADDING * 2);
            double right = left + width;
            double bottom = top + height;

            freqGraph.Width = width;

            //draw axis
            DrawTools.DrawLine(freqGraph, left, left, top, bottom, freqChartBrush);
            DrawTools.DrawLine(freqGraph, left, right, bottom, bottom, freqChartBrush);

            //draw y axis numbers
            for (int i = 0; i < height + Y_GAP; i += Y_GAP)
			{
				DrawTools.Text(freqGraph, 0, bottom - i, "" + Math.Round(i * signal.MaxAmp / height, 0), freqChartBrush);
			}

            // draw x axis numbers
            for (int i = 0; i < frequencies.Length; i++)
			{
                DrawTools.DrawBar(freqGraph, left + i * X_GAP, bottom, 8, frequencies[i] * (height / signal.MaxAmp), freqChartBrush);
                DrawTools.Text(freqGraph, left + i * X_GAP, bottom, Math.Round(i * binSize, 0) + "", freqChartBrush);
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
