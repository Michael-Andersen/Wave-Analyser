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
        public FrequencyWindow()
        {
            InitializeComponent();
			FrequencyViewer fv = new FrequencyViewer();
			fv.GenerateFreqData();
			fv.DrawGraph(10, 7000);
			content.Children.Add(fv);
		}
    }
}
