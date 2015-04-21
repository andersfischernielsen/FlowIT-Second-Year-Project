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
using Client.ViewModels;

namespace Client.Views
{
    /// <summary>
    /// Interaction logic for HistoryView.xaml
    /// </summary>
    public partial class HistoryView : Window
    {
        public HistoryView()
        {
            InitializeComponent();
        }

        public HistoryView(HistoryListViewModel history)
        {
            DataContext = history;
            InitializeComponent();
        }
    }
}
