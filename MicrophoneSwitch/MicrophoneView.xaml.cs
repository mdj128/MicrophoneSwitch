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

namespace MicrophoneSwitch
{
    /// <summary>
    /// Interaction logic for MicrophoneView.xaml
    /// </summary>
    public partial class MicrophoneView
    {
        public MicrophoneView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as MicrophoneViewModel).Disable();
            this.Visibility = Visibility.Collapsed;
        }
    }
}
