using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Views;
using Common;

namespace Client.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        public Action CloseAction { get; set; }
        public LoginViewModel()
        {
            
        }

        #region Databindings
        private string _username = "Username";
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                NotifyPropertyChanged("Username");
            }
        }

        private string _password = "Password";
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                NotifyPropertyChanged("Password");
            }
        }
        #endregion

        #region Actions

        public void Login()
        {
            var window = new MainWindow();
            window.Show();
            CloseAction.Invoke();
        }

        #endregion
    }
}
