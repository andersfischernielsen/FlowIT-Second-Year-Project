using System;
using Client.Views;

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

        private string _status = "";
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                NotifyPropertyChanged("Status");
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

        public async void Login()
        {
            // PUT LOGIN LOGIC HERE
            Status = "";
            var connection = new ServerConnection(new Uri("http://localhost:13768/"));
            try
            {
                var roles = await connection.Login(Username);
                Status = "Login successful";
                EventConnection.RoleForWorkflow = roles.RolesOnWorkflows;

                var window = new MainWindow();
                window.Show();
                CloseAction.Invoke();
            }
            catch (Exception)
            {
                Status = "Login failed";
            }
            
        }

        #endregion
    }
}
