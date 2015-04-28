using System.Windows;
using Client.ViewModels;

namespace Client.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
            var vm = new LoginViewModel(); // this creates an instance of the ViewModel
            DataContext = vm; // this sets the newly created ViewModel as the DataContext for the View
            if (vm.CloseAction == null)
                vm.CloseAction = Close;
        }
    }
}
