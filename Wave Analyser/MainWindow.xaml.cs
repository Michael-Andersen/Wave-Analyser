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
        public readonly double SAMPLE_RATE = 44100.0;
        public readonly int BIT_DEPTH = 16;

		public MainWindow()
		{
			InitializeComponent();

      WaveformViewer waveformViewer = new WaveformViewer(SAMPLE_RATE, BIT_DEPTH);
      waveformViewer.GenerateRandomSamples(100);
      content.Children.Add(waveformViewer);
      waveformViewer.DrawGraph(1);

        }

		

	}
}
