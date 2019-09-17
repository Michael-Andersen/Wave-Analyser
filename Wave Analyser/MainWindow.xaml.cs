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
		public MainWindow()
		{
			InitializeComponent();
			Random r = new Random();
			//Mock random sample data of 10 secs worth of 44100 samples/sec 
			int[] points = new int[44100 * 10];
			for (int i = 0; i < points.Length; i++)
			{
				points[i] = r.Next(0, (int)timeDomainCanvas.Height);
			}
			zoom2(timeDomainCanvas, 1.1);
			drawGraph(points, 10);
		}

		//uses lines between sample points to draw graph, uses spaces variable to determine which of
		//the points to plot ie. plot every 10th point
		private void drawGraph(int[] points, int spaces)
		{
			for (int i = 0; i < points.Length - spaces; i += spaces)
			{
				Line segement = new Line();
				segement.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
				segement.X1 = i * spaces * timeDomainCanvas.Width / points.Length;
				segement.X2 = (i + 1) * spaces * timeDomainCanvas.Width / points.Length;
				segement.Y1 = points[i];
				segement.Y2 = points[i + spaces];
				segement.StrokeThickness = 2;
				timeDomainCanvas.Children.Add(segement);
			}
		}

		//rename to zoom 
		private void zoom2(Canvas canvas, double xfactor)
		{
			ScaleTransform st = new ScaleTransform();
			canvas.RenderTransform = st;
			canvas.MouseWheel += (sender, e) =>
			{
				if (e.Delta > 0)
				{
					st.ScaleX *= xfactor;
				}
				else
				{
					st.ScaleX /= xfactor;
				}
			};
		}
	}
}
