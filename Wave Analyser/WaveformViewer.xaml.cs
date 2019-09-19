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
        private double zoom;
        private int[] samples;

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

            Measure(new Size(1000, 1000));
            Arrange(new Rect(0, 0, 1000, 1000));

            // draw the zero line
            double backkgroundHeight = background.ActualHeight / 2;
            DrawLine(background, 0, background.ActualWidth, backkgroundHeight, backkgroundHeight,
                System.Windows.Media.Brushes.LightSteelBlue);
        }

        // uses lines between sample points to draw graph, uses spaces variable to determine which of
        // the points to plot ie. plot every 10th point
        public void DrawGraph(int spaces = 1)
        {
            if (samples == null) return;

            for (int i = 0; i < samples.Length - spaces; i += spaces)
            {
                double y1 = ((double)(samples[i] - minAmp) / (maxAmp - minAmp)) * ActualHeight;
                double y2 = ((double)(samples[i + 1] - minAmp) / (maxAmp - minAmp)) * ActualHeight;

                DrawLine(timeDomainGraph,
                    i * spaces,
                    (i + 1) * spaces,
                    y1,
                    y2,
                    System.Windows.Media.Brushes.SteelBlue);
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

        public void GenerateRandomSamples(double seconds)
        {
            Random random = new Random();
            timeDomainGraph.Width = sampleRate * seconds;
            int numSamples = (int)(sampleRate * seconds);
            samples = new int[numSamples];

            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = random.Next(minAmp, maxAmp + 1);
            }
        }
    }
}
