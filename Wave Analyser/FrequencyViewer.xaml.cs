﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        public static readonly int BAR_WIDTH = 4;

		private int selectStart;
		private int selectEnd;
		bool mouseDown = false;
		Point mouseDownPos;
		private double scale;
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
			scale = 1.0 / N;
            maxFreq = 0;
			NumThreads = 4;
            this.SizeChanged += FrequencyViewer_SizeChanged;
        }

        private void FrequencyViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (frequencies == null)
                return;

            DrawGraph();
        }

        public AudioFile Audio { set => audio = value; }

		public int NumThreads { get; set; }
        public int NumBins { get; set; }


		public int SelectStart { get => selectStart; }
		public int SelectEnd { get => selectEnd; }

		public void DrawGraph()
		{
            freqGraph.Children.Clear();
            freqGraph.UpdateLayout();

            MeasureGraph();
			scale = (NumBins >= N) ? 1.0 / N : 1.0 / NumBins;
			double xGap = (scale * NumBins) * width / frequencies.Length;
			//double xGap =  width / frequencies.Length;
			double xGapA =  width / NUM_X;
			//double xGapA = (scale * NumBins) * width / NUM_X;
			freqGraph.Width = left + xGap * (frequencies.Length + 1) + xGapA;
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
		    
			//xGap = 50;

            for (int i = 0; i < frequencies.Length; i++)
			{
				DrawTools.DrawBar(freqGraph, left + i * xGap, bottom, BAR_WIDTH, frequencies[i] * (height / maxFreq), freqChartBrush);
            }

			// draw x axis numbers

			int numLabels = (int)(scale * NumBins * NUM_X);
            for (int i = 0; i < numLabels + 1; i++)// NUM_X + 1
            {
                int frequency = (i * audio.SampleRate / numLabels);  
                DrawTools.Text(freqGraph, left + i * xGapA, bottom, frequency + "", freqChartBrush);
            }
		}

		public void GenerateFromFourier(float[] samples)
		{
			//Complex[] fourierResults = Fourier.DFT(samples, NumBins);
			Complex[] fourierResults = Fourier.DFT_Thread(samples, NumBins, NumThreads);
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

		private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (audio == null)
			{
				return;
			}
			mouseDownPos = e.GetPosition(freqGraph);
			if (e.ChangedButton == MouseButton.Right)
			{
				return;
			}
			mouseDown = true;
			theGrid.CaptureMouse();

			Canvas.SetLeft(selectionBox, mouseDownPos.X);
			Canvas.SetTop(selectionBox, mouseDownPos.Y);
			Canvas.SetLeft(selectionBoxMirror, freqGraph.ActualWidth - (mouseDownPos.X - left) + left);
			Canvas.SetTop(selectionBoxMirror, mouseDownPos.Y);
			selectionBox.Width = 0;
			selectionBox.Height = freqGraph.ActualHeight;
			selectionBoxMirror.Width = 0;
			selectionBoxMirror.Height = freqGraph.ActualHeight;
			selectionBox.Visibility = Visibility.Visible;
			selectionBoxMirror.Visibility = Visibility.Visible;
		}

		public void ClearSelection()
		{
			selectionBox.Visibility = Visibility.Collapsed;
			selectionBoxMirror.Visibility = Visibility.Collapsed;
		}

		private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (audio == null)
			{
				return;
			}
			if (e.ChangedButton == MouseButton.Right)
			{
				return;
			}
			mouseDown = false;
			theGrid.ReleaseMouseCapture();

			selectionBox.Opacity = 0.2;
			selectionBox.Fill = Brushes.PaleVioletRed;
			selectionBoxMirror.Opacity = 0.2;
			selectionBoxMirror.Fill = Brushes.PaleVioletRed;
			Point mouseUpPos = e.GetPosition(theGrid);
			if (mouseUpPos.X > mouseDownPos.X)
			{
				selectStart = (int)((mouseDownPos.X-left) / ((scale * NumBins) * width / NumBins));
				if (selectStart < 0)
				{
					selectStart = 0;
				}
				selectEnd = (int)((mouseUpPos.X-left) / ((scale * NumBins) * width / NumBins));
			}
			else
			{
				selectStart = (int)((mouseUpPos.X -left) / ((scale * NumBins) * width / NumBins));
				if (selectStart < 0)
				{
					selectStart = 0;
				}
				selectEnd = (int)((mouseDownPos.X - left) / ((scale * NumBins) *width / NumBins));
			}
			int freqS = selectStart * audio.SampleRate / NumBins;
			int freqE = selectEnd * audio.SampleRate / NumBins;
		}

		private void Grid_MouseMove(object sender, MouseEventArgs e)
		{
			if (mouseDown)
			{
				Point mousePos = e.GetPosition(theGrid);
				double mirrorPos;
				if (mouseDownPos.X < mousePos.X)
				{
					Canvas.SetLeft(selectionBox, mouseDownPos.X);
					mirrorPos = (scale * NumBins) * width - (mousePos.X - left) + left;
					Canvas.SetLeft(selectionBoxMirror,
						mirrorPos);
					selectionBox.Width = mousePos.X - mouseDownPos.X;
					selectionBoxMirror.Width = mousePos.X - mouseDownPos.X;
				}
				else
				{
					Canvas.SetLeft(selectionBox, mousePos.X);
					mirrorPos = (scale * NumBins) * width - (mouseDownPos.X - left) + left;
					Canvas.SetLeft(selectionBoxMirror,
						mirrorPos);
					selectionBox.Width = mouseDownPos.X - mousePos.X;
					selectionBoxMirror.Width = mouseDownPos.X - mousePos.X;
				}
				Canvas.SetTop(selectionBox, 0);
				Canvas.SetTop(selectionBoxMirror, 0);

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
