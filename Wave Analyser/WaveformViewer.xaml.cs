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
        private static readonly int DEFAULT_ZOOM = 128;

		private AudioFile audio;
		private WaveformViewer otherChannel;
		private int selectStart;
		private int selectEnd;
		bool mouseDown = false;
		Point mouseDownPos;
		bool isLeftChannel;
        private Brush waveformBrush = (Brush)Application.Current.FindResource("waveformBrush");
        private Brush zeroLineBrush = (Brush)Application.Current.FindResource("zeroLineBrush");

        public WaveformViewer(bool isLeftChannel)
        {
            InitializeComponent();
			this.isLeftChannel = isLeftChannel;
			if (isLeftChannel)
			{
				viewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden; 
			}
            DrawTools.Zoom = DEFAULT_ZOOM;
            timeGraph.MouseWheel += MouseWheelZoom;
            viewer.ScrollChanged += ScrollChanged;
        }

        public AudioFile AudioFile { set => audio = value; }
        public int SelectStart { get => selectStart; }
        public int SelectEnd { get => selectEnd; }
		public WaveformViewer OtherChannel { set => otherChannel = value; }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
		{
			mouseDown = true;
			mouseDownPos = e.GetPosition(theGrid);
			theGrid.CaptureMouse();

			Canvas.SetLeft(selectionBox, mouseDownPos.X);
			Canvas.SetTop(selectionBox, mouseDownPos.Y);
			selectionBox.Width = 0;
			selectionBox.Height = timeGraph.ActualHeight;

			selectionBox.Visibility = Visibility.Visible;
		}

		public void ClearSelection()
		{
			selectionBox.Visibility = Visibility.Collapsed;
		}

		private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
		{
			mouseDown = false;
			theGrid.ReleaseMouseCapture();

			selectionBox.Opacity = 0.2;
			selectionBox.Fill = Brushes.PaleVioletRed;
			Point mouseUpPos = e.GetPosition(theGrid);
            audio.LeftSelected = isLeftChannel;
			otherChannel.ClearSelection();
			if (mouseUpPos.X > mouseDownPos.X)
			{
				selectStart = (int) mouseDownPos.X * DrawTools.Zoom ;
				selectEnd = (int) mouseUpPos.X * DrawTools.Zoom ;
			} else
			{
				selectStart = (int) mouseUpPos.X * DrawTools.Zoom;
				selectEnd = (int) mouseDownPos.X * DrawTools.Zoom;
			}
		}

		private void Grid_MouseMove(object sender, MouseEventArgs e)
		{
			if (mouseDown)
			{
				Point mousePos = e.GetPosition(theGrid);

				if (mouseDownPos.X < mousePos.X)
				{
					Canvas.SetLeft(selectionBox, mouseDownPos.X);
					selectionBox.Width = mousePos.X - mouseDownPos.X;
				}
				else
				{
					Canvas.SetLeft(selectionBox, mousePos.X);
					selectionBox.Width = mouseDownPos.X - mousePos.X;
				}
					Canvas.SetTop(selectionBox, 0);
				
			}
		}
        public void DrawGraph()
        {
            if (audio?.Samples == null)
                return;

            timeGraph.Children.Clear();
            timeGraph.UpdateLayout();
            timeAxis.Children.Clear();
            timeAxis.UpdateLayout();
			DrawAxis();
            DrawWaveform();
        }

		private void DrawAxis()
		{
			//draw the zero line
			double y = timeGraph.ActualHeight / 2;
			DrawTools.DrawLine(timeGraph, viewer.HorizontalOffset, viewer.HorizontalOffset + viewer.ActualWidth, y, y, zeroLineBrush);
			if (!isLeftChannel) { 
				//draw the time axis
				DrawTools.DrawLine(timeAxis, viewer.HorizontalOffset, viewer.HorizontalOffset + viewer.ActualWidth, 0, 0, waveformBrush, 2);

				//label times
				for (int i = 0; i < viewer.ActualWidth + 20; i += 80)
				{
					double timeDisplay = Math.Round((i + viewer.HorizontalOffset) * DrawTools.Zoom / audio.SampleRate, 3);
					DrawTools.DrawLine(timeAxis, viewer.HorizontalOffset + i, viewer.HorizontalOffset + i + 0.5, 0, 5, waveformBrush);
					DrawTools.Text(timeAxis, viewer.HorizontalOffset + i, 0, "" + timeDisplay, waveformBrush);
				}
		}
        }

        public void DrawWaveform()
        {
			int spaces = DrawTools.Zoom;
			float[] samps = (isLeftChannel) ? audio.Left : audio.Right;
            timeGraph.Width = (samps.Length / spaces > viewer.ActualWidth) ? 
				samps.Length  / spaces : viewer.ActualWidth;	
			
			int xpos = (int) viewer.HorizontalOffset;

            for (int i = (int)viewer.HorizontalOffset; i < audio.Left.Length - spaces; i += spaces)
            {
				if (xpos - viewer.HorizontalOffset >= viewer.ActualWidth)
				{
					break; //stop drawing when out of view
				}

                double y1 = GetSampleY(samps[i]); //for test using left
			    double y2 = GetSampleY(samps[i + spaces]);
                DrawTools.DrawLine(timeGraph, xpos, (xpos + 1), y1, y2, waveformBrush);
				xpos++;
            }			
        }

        private double GetSampleY(double sample)
        {   
			return ((sample - audio.MinAmp) / (audio.MaxAmp - audio.MinAmp)) * timeGraph.ActualHeight;
        }

		public void ScrollChanged(Object sender, ScrollChangedEventArgs e)
		{
			DrawGraph();
			otherChannel.Scroll(viewer.HorizontalOffset);
		}

		public void Scroll(double scroll)
		{
			viewer.ScrollToHorizontalOffset(scroll);
		}
        public void MouseWheelZoom(Object sender, MouseWheelEventArgs e)
        {
			{
				int zoomFactor = 2;

				if (e.Delta > 0)
				{
					if (DrawTools.Zoom <= 1)
					{
						DrawTools.Zoom = 1;
						return; //max zoom in is not skipping any signal.Samples
					}
					DrawTools.Zoom = (DrawTools.Zoom / zoomFactor);
					viewer.ScrollToHorizontalOffset(viewer.HorizontalOffset * zoomFactor);
					otherChannel.DrawGraph();
					DrawGraph();
				}
				else
				{
					if (DrawTools.Zoom * zoomFactor > audio.Left.Length / 16)
					{
						return; //max zoom out is only drawing 16 signal.Samples
					}
					DrawTools.Zoom = (DrawTools.Zoom * zoomFactor);
					viewer.ScrollToHorizontalOffset(viewer.HorizontalOffset / zoomFactor);
					otherChannel.DrawGraph();
					DrawGraph();
				}
			}
        }
    }
}
