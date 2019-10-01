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
		int start;
		int finish;
		FrequencyViewer fv;

		public FrequencyWindow()
        {
            InitializeComponent();
			fv = new FrequencyViewer();
			content.Children.Add(fv);
			
		}

		public void init()
		{
			
			fv.GenerateFromFourier(samples, samples.Length);
			fv.DrawGraph(Fourier.BinSize(WaveformViewer.sampleRate, samples.Length), 44100);
			
		}

		public void setSamples(int[] samples)
		{
			int j = 0;
			this.samples = new int[finish - start];
			for (int i = start; i < finish; i++)
			{
				this.samples[j++] = samples[i];
			}
		}

		public void setStartandFinish(int s, int f)
		{
			start = s;
			finish = f;
		}
    }
}
