using System.Diagnostics;
using System.Windows;

namespace DcrParserGraphic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void hiddenbutton_onclick(object sender, RoutedEventArgs e)
        {
            Process.Start(@"http://www.staggeringbeauty.com/");
        }

    }
}
