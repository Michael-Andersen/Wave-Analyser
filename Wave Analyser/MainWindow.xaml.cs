﻿using System.Windows;
using Wave_Analyser.Classes;

namespace Wave_Analyser
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private LibLink libLink;

		public MainWindow()
		{
			InitializeComponent();
			
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
	}
}
