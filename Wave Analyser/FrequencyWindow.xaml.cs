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
using System.Windows.Shapes;

namespace Wave_Analyser
{
    /// <summary>
    /// Interaction logic for FrequencyWindow.xaml
    /// </summary>
    public partial class FrequencyWindow : Window
    {
		int[] samples;

        public FrequencyWindow()
        {
            InitializeComponent();
			
		}

		public void init()
		{
			FrequencyViewer fv = new FrequencyViewer();
			fv.GenerateFromFourier(samples, 2000);
			fv.DrawGraph(Fourier.BinSize(WaveformViewer.sampleRate, 2000), 7000);
			content.Children.Add(fv);
		}

		public void setSamples(int[] samples)
		{
			this.samples = samples;
		}
    }
}
