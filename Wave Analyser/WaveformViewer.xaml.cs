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
    /// Interaction logic for WaveformViewer.xaml
    /// </summary>
    public partial class WaveformViewer : UserControl
    {
        private double sampleRate;
        private int bitDepth;
        private int maxAmp;
        private int minAmp;
		private int zoom;
        private double[] samples;

        public WaveformViewer(double sampleRate, int bitDepth)
        {
            
            Init(sampleRate, bitDepth);
        }

        public WaveformViewer()
        {
            Init(44100, 16);
        }

        public void Init(double sampleRate, int bitDepth)
        {
            InitializeComponent();
            this.sampleRate = sampleRate;
            this.bitDepth = bitDepth;
            maxAmp = (int)Math.Pow(2, bitDepth - 1);
            minAmp = -maxAmp--;
			zoom = 128;
            Measure(new Size(1000, 1000));
            Arrange(new Rect(0, 0, 1000, 1000));

            // draw the zero line
            double backkgroundHeight = background.ActualHeight / 2;
            DrawLine(background, 0, background.ActualWidth, backkgroundHeight, backkgroundHeight,
                System.Windows.Media.Brushes.LightSteelBlue);
			timeDomainGraph.MouseWheel += MouseWheelZoom;
			viewer.ScrollChanged += ScrollChanged;
			
			
        }

        // uses lines between sample points to draw graph, uses spaces variable to determine which of
        // the points to plot ie. plot every 10th point
        public void DrawGraph()
        {
			if (timeDomainGraph.Width != samples.Length)
			{
				timeDomainGraph.Width = samples.Length;
			}
			int spaces = zoom;
				
			if (samples == null) return;
			int xpos = (int) viewer.HorizontalOffset;
            for (int i = (int)viewer.HorizontalOffset; i < samples.Length - spaces; i += spaces)
            {
				
				if (xpos - viewer.HorizontalOffset >= background.ActualWidth)
				{
					break;
				}
				double y1 = ((double)(samples[i] - minAmp) / (maxAmp - minAmp)) * ActualHeight;
			    double y2 = ((double)(samples[i + spaces] - minAmp) / (maxAmp - minAmp)) * ActualHeight;
                DrawLine(timeDomainGraph,
                    xpos,
                    (xpos + 1),
                    y1,
                    y2,
                    System.Windows.Media.Brushes.SteelBlue);
				xpos++;
            }
        }

		private void ScrollChanged(Object sender, ScrollChangedEventArgs e)
		{
			
			timeDomainGraph.Children.Clear();
			timeDomainGraph.UpdateLayout();
			DrawGraph();
			
		}
		private void MouseWheelZoom(Object sender, MouseWheelEventArgs e)
		{
			int zoomFactor = 2;
		
			if (e.Delta > 0)
			{
				if (zoom <= 1)
				{
					zoom = 1;
					return; // max zoom in is not skipping any samples
				}
				zoom = (zoom / zoomFactor);
				timeDomainGraph.Children.Clear();
				timeDomainGraph.UpdateLayout();
				DrawGraph();
			}
			else
			{	
				if (zoom * zoomFactor > samples.Length / 16)
				{
					return; // max zoom out is only drawing 16 samples
				}
				zoom = (zoom * zoomFactor);
				int debugzoom = zoom;
				timeDomainGraph.Children.Clear();
				timeDomainGraph.UpdateLayout();
				DrawGraph();
			}
		}
		
		private void DrawLine(Canvas canvas, double x1, double x2, double y1, double y2, Brush color, double thickness = 1)
        {
            Line line = new Line();
            line.X1 = x1;
            line.X2 = x2;
            line.Y1 = y1;
            line.Y2 = y2;
            line.Stroke = color;
            line.StrokeThickness = thickness;
            canvas.Children.Add(line);
        }

		private void ViewerPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			var clickedElement = e.OriginalSource;
		}

		public void GenerateSineData(double seconds)
		{
			Random random = new Random();
			int[] freqs = new int[10]; 
			for (int i = 0; i < 10; i++)
			{
				freqs[i] = random.Next(1, 5000);
			}
			
			samples = new double[(int)(sampleRate*seconds)];
			for (int i = 0; i < sampleRate * seconds; i++)
			{
				double time = i / sampleRate;
				samples[i] = 0;
				for (int j = 0; j < freqs.Length; j++)
				{
					samples[i] += (maxAmp) * (1.0/(freqs.Length)) * Math.Sin(2 * Math.PI * freqs[j] * time);
				}
			}
		}
		

		public void GenerateRandomSamples(double seconds)
        {
            Random random = new Random();
            int numSamples = (int)(sampleRate * seconds);
            samples = new double[numSamples];

            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = random.Next(minAmp, maxAmp + 1);
            }
        }
    }
}
