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
using Wave_Analyser.Classes;

namespace Wave_Analyser
{
    /// <summary>
    /// Interaction logic for WaveformViewer.xaml
    /// </summary>
    public partial class WaveformViewer : UserControl
    {
        public static readonly int DEFAULT_ZOOM = 128;

        private AudioSignal signal;
		private int zoom;

        public WaveformViewer()
        {
            InitializeComponent();

            zoom = DEFAULT_ZOOM;
            timeDomainGraph.MouseWheel += MouseWheelZoom;
            viewer.ScrollChanged += ScrollChanged;
        }

        public AudioSignal Signal { set => signal = value; }

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
				double timeDisplay = Math.Round((i + viewer.HorizontalOffset) * zoom / signal.SampleRate, 3);
				DrawTools.DrawLine(background, i, i + 0.5, background.ActualHeight - 10, background.ActualHeight - 15,
					Brushes.Black);
				DrawTools.Text(background, i, background.ActualHeight - 15,
					"" + timeDisplay,
					Brushes.Black);
			}
		}

		public void DrawWaveform()
        {
            if (signal?.Samples == null)
                return;

            //remove old waveform
            timeDomainGraph.Children.Clear();
            timeDomainGraph.UpdateLayout();
			int spaces = zoom;
			timeDomainGraph.Width = (signal.Samples.Length / spaces > background.ActualWidth) ? 
				signal.Samples.Length  / spaces : background.ActualWidth;	
			
			int xpos = (int) viewer.HorizontalOffset;
            for (int i = (int)viewer.HorizontalOffset; i < signal.Samples.Length - spaces; i += spaces)
            {
				if (xpos - viewer.HorizontalOffset >= background.ActualWidth)
				{
					break; //stop drawing when out of view
				}

                double y1 = GetSampleY(signal.Samples[i]);
			    double y2 = GetSampleY(signal.Samples[i + spaces]);
                DrawTools.DrawLine(timeDomainGraph, xpos, (xpos + 1), y1, y2, Brushes.SteelBlue);
				xpos++;
            }			
        }

        private double GetSampleY(int sample)
        {
            if (signal.Signed)
            {
                return ((double)(sample - signal.MinAmp) / (signal.MaxAmp - signal.MinAmp)) * background.ActualHeight;
            }

            return 0.0;
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
                    return; //max zoom in is not skipping any signal.Samples
                }
                zoom = (zoom / zoomFactor);
                viewer.ScrollToHorizontalOffset(viewer.HorizontalOffset * zoomFactor);
                DrawGraph();
            }
            else
            {
                if (zoom * zoomFactor > signal.Samples.Length / 16)
                {
                    return; //max zoom out is only drawing 16 signal.Samples
                }
                zoom = (zoom * zoomFactor);
                viewer.ScrollToHorizontalOffset(viewer.HorizontalOffset / zoomFactor);
                DrawGraph();
            }
        }
    }
}
