using System;
using System.Collections.Generic;
using Client.Connections;
using Client.Views;

namespace Client.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        public Action CloseAction { get; set; }       
        public static Dictionary<string, IList<string>> RoleForWorkflow { get; set; }

        private bool _loginStarted;
        private readonly Uri _serverAddress;

        public LoginViewModel()
        {
            var settings = Settings.LoadSettings();
            Username = settings.Username;
            _serverAddress = new Uri(settings.ServerAddress);

            _status = "";
            _password = "Password";
        }

        #region Databindings

        private string _username;
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                NotifyPropertyChanged("Username");
            }
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                NotifyPropertyChanged("Status");
            }
        }
        private string _password;
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
            if (_loginStarted) return;
            _loginStarted = true;
            Status = "Attempting login...";

            // PUT LOGIN LOGIC HERE
            IServerConnection connection = new ServerConnection(_serverAddress);
            try
            {
                var roles = await connection.Login(Username, Password);
                Status = "Login successful";
                RoleForWorkflow = roles.RolesOnWorkflows;

                // Save settings
                var settings = new Settings
                {
                    ServerAddress = _serverAddress.AbsoluteUri,
                    Username = _username,
                };
                
                
                settings.SaveSettings();
                // Save settings end.

                

                var window = new MainWindow();
                window.Show();
                CloseAction.Invoke();
            }
            catch (Exception ex)
            {
                _loginStarted = false;
                Status = ex.Message;
            }
            
        }

        #endregion

        
    }
}
