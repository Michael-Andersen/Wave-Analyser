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
        public static double sampleRate;
        private int bitDepth;
        public static int maxAmp;
        private int minAmp;
		private int zoom;
        private int[] samples;
        private Random random;

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
            WaveformViewer.sampleRate = sampleRate;
            this.bitDepth = bitDepth;
            maxAmp = (int)Math.Pow(2, bitDepth - 1);
            minAmp = -maxAmp--;
			zoom = 128;
            random = new Random();
            Measure(new Size(1000, 1000));
            Arrange(new Rect(0, 0, 1000, 1000));
			timeDomainGraph.MouseWheel += MouseWheelZoom;
			viewer.ScrollChanged += ScrollChanged;
        }

        public void DrawGraph()
        {
            DrawAxis();
            DrawWaveform();
        }

		private void DrawAxis()
		{
			//remove old axis
			background.Children.Clear();
			background.UpdateLayout();
			//draw the zero line
			double backgroundHeight = background.ActualHeight / 2;
			DrawTools.DrawLine(background, 0, background.ActualWidth + 20, backgroundHeight, backgroundHeight,
				Brushes.LightSteelBlue);
			//draw the time axis
			DrawTools.DrawLine(background, 0, background.ActualWidth + 20,
			background.ActualHeight - 15, background.ActualHeight - 15, Brushes.Black, 2);
			//label times
			for (int i = 0; i < background.ActualWidth + 20; i += 80)
			{
				double timeDisplay = Math.Round((i + viewer.HorizontalOffset) * zoom / sampleRate, 3);
				DrawTools.DrawLine(background, i, i + 0.5, background.ActualHeight - 10, background.ActualHeight - 15,
					Brushes.Black);
				DrawTools.Text(background, i, background.ActualHeight - 15,
					"" + timeDisplay,
					Brushes.Black);
			}
		}

		public int[] getSamples()
		{
			return samples;
		}

		public void DrawWaveform()
        {
            if (samples == null) return;
            //remove old waveform
            timeDomainGraph.Children.Clear();
            timeDomainGraph.UpdateLayout();
			int spaces = zoom;
			timeDomainGraph.Width = (samples.Length / spaces > background.ActualWidth) ? 
				samples.Length  / spaces : background.ActualWidth;	
			
			int xpos = (int) viewer.HorizontalOffset;
            for (int i = (int)viewer.HorizontalOffset; i < samples.Length - spaces; i += spaces)
            {
				if (xpos - viewer.HorizontalOffset >= background.ActualWidth)
				{
					break; //stop drawing when out of view
				}
				
				double y1 = ((double)(samples[i] - minAmp) / (maxAmp - minAmp)) * background.ActualHeight;
			    double y2 = ((double)(samples[i + spaces] - minAmp) / (maxAmp - minAmp)) * background.ActualHeight;
                DrawTools.DrawLine(timeDomainGraph,
                    xpos,
                    (xpos + 1),
                    y1,
                    y2,
                    Brushes.SteelBlue);
				xpos++;
            }			
        }

		private void ScrollChanged(Object sender, ScrollChangedEventArgs e)
		{
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
					return; //max zoom in is not skipping any samples
				}
				zoom = (zoom / zoomFactor);
				viewer.ScrollToHorizontalOffset(viewer.HorizontalOffset * zoomFactor);
                DrawGraph();
			}
			else
			{	
				if (zoom * zoomFactor > samples.Length / 16)
				{
					return; //max zoom out is only drawing 16 samples
				}
				zoom = (zoom * zoomFactor);
				viewer.ScrollToHorizontalOffset(viewer.HorizontalOffset / zoomFactor);
                DrawGraph();
			}
		}
		
		public void GenerateSineData(double seconds, int[] freqs)
		{
			
			samples = new int[(int)(sampleRate*seconds)];
			for (int i = 0; i < samples.Length; i++)
			{
				double time = i / sampleRate;
				samples[i] = 0;
				for (int j = 0; j < freqs.Length; j++)
				{
                    double amp = (maxAmp) / freqs.Length * Math.Sin(2 * Math.PI * freqs[j] * time);
                    samples[i] += (int)amp;
					
				}
			}
		}

        public void GenerateSineTone(double seconds, double frequency)
        {
            samples = new int[(int)(sampleRate * seconds)];
            for (int i = 0; i < samples.Length; i++)
            {
                double time = i / sampleRate;
                double amp = maxAmp * Math.Sin(2 * Math.PI * frequency * time);
                samples[i] = (int)amp;
            }
        }
		
		public void GenerateRandomSamples(double seconds)
        {
            samples = new int[(int)(sampleRate * seconds)];
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = random.Next(minAmp, maxAmp + 1);
            }
        }
    }
}
