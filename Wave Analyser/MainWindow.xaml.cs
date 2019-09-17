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
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
        int sampleRate = 44100;
        int zoom = 2;

		public MainWindow()
		{
			InitializeComponent();
            Random r = new Random();

			//Mock random sample data
			int[] points = new int[sampleRate * 1];

			for (int i = 0; i < points.Length; i++)
			{
				points[i] = r.Next(0, (int)timeDomainCanvas.Height);
			}

            int timeDomainWidth = sampleRate * zoom;
            timeDomainCanvas.Width = timeDomainWidth;

			DrawGraph(points, 1);
		}

        //uses lines between sample points to draw graph, uses spaces variable to determine which of
        //the points to plot ie. plot every 10th point
        private void DrawGraph(int[] points, int spaces)
		{
			for (int i = 0; i < points.Length - spaces; i += spaces)
			{
				Line segment = new Line();
				segment.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
                segment.X1 = i * spaces * zoom;
                segment.X2 = (i + 1) * spaces * zoom;
                segment.Y1 = points[i];
                segment.Y2 = points[i + spaces];
                segment.StrokeThickness = 2;
				timeDomainCanvas.Children.Add(segment);
			}
		}
	}
}
