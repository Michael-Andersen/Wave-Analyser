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
    /// Interaction logic for WaveWindow.xaml
    /// </summary>
    public partial class WaveWindow : Window
    {
		private LibLink libLink;
		private MainWindow mainWin;

		public WaveWindow(MainWindow mainWin)
		{
			InitializeComponent();
			this.mainWin = mainWin;
			libLink = new LibLink(this, controlBox);
			controlBox.LibLink = libLink;
			WaveformViewer waveformViewerL = new WaveformViewer(true);
			WaveformViewer waveformViewerR = new WaveformViewer(false);
			waveformViewerL.OtherChannel = waveformViewerR;
			waveformViewerR.OtherChannel = waveformViewerL;
			waveformPanel.Children.Add(waveformViewerL);
			waveformPanel2.Children.Add(waveformViewerR);
			controlBox.WaveformViewerL = waveformViewerL;
			controlBox.WaveformViewerR = waveformViewerR;
			controlBox.FrequencyViewer = freqDomain;
			controlBox.windowingSelect.ItemsSource = System.Enum.GetValues(typeof(WindowingMode));
			controlBox.windowingSelect.SelectedItem = controlBox.WindowingMode;
		}

		private void OpenFile_Click(object sender, RoutedEventArgs e)
		{
			controlBox.OpenFile();
		}

		private void SaveFile_Click(object sender, RoutedEventArgs e)
		{
			controlBox.SaveFile();
		}

		private void New_Click(object send, RoutedEventArgs e)
		{
			mainWin.addWaveWindow();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			mainWin.removeWaveWindow(this);
		}
	}

}
