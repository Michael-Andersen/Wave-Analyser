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
		double[] frequencies;
		public FrequencyViewer()
		{
			InitializeComponent();
			Measure(new Size(1000, 1000));
			Arrange(new Rect(0, 0, 1000, 1000));
	
		}
		private void Text(Canvas canvas, double x, double y, string text, Brush colour)
		{
			TextBlock textBlock = new TextBlock();
			textBlock.Text = text;
			textBlock.Foreground = colour;
			Canvas.SetLeft(textBlock, x);
			Canvas.SetTop(textBlock, y);
			canvas.Children.Add(textBlock);
		}
		private void DrawBar(Canvas canvas, double frequency, double amplitude, double width)
		{
			double bottom = 200;
			Rectangle bar = new Rectangle();
			bar.Stroke = Brushes.DarkGreen;
			bar.Fill = Brushes.DarkGreen;
			bar.Width = 8;
			bar.Height = amplitude;
			Canvas.SetLeft(bar, frequency);
			Canvas.SetTop(bar, bottom - amplitude);
			canvas.Children.Add(bar);
		}

		public void DrawGraph(int binSize, int maxFreq)
		{

			frequencyDomainGraph.Width = maxFreq * 30.0 / binSize;
			for (int i = 0; i < frequencies.Length; i++)
			{
				DrawBar(frequencyDomainGraph, 30 + (i) * 30, frequencies[i], 10);
				Text(frequencyDomainGraph, 30 + i * 30, 215, i * binSize + "", Brushes.Black);
			}
			double z = fViewer.ViewportWidth;
			fViewer.ScrollToEnd();
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
