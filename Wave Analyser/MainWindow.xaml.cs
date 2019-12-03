using System.Collections.Generic;
using System.Windows;
using Wave_Analyser.Classes;

namespace Wave_Analyser
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private List<WaveWindow> waves;

		public MainWindow()
		{
			InitializeComponent();
			WaveWindow ww = new WaveWindow(this);
			this.Hide();
			waves = new List<WaveWindow>();
			ww.Show();
			waves.Add(ww);
			
		}

		public void addWaveWindow()
		{
			WaveWindow ww = new WaveWindow(this);
			waves = new List<WaveWindow>();
			ww.Show();
			waves.Add(ww);
		}

		public void removeWaveWindow(WaveWindow ww)
		{
			waves.Remove(ww);
			if (waves.Count == 0)
			{
				this.Close();
			}
		}
	}
}
