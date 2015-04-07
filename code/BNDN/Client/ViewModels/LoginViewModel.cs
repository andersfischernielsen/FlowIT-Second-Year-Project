using System;
using System.IO;
using Client.Views;
using Newtonsoft.Json;

namespace Client.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        public Action CloseAction { get; set; }
        private bool _loginStarted;
        private string _password;
        private string _status;
        private string _username;
        private readonly Uri _serverAddress;
        public LoginViewModel()
        {
            if (File.Exists("settings.json"))
            {
                var settingsjson = File.ReadAllText("settings.json");
                var settings = JsonConvert.DeserializeObject<Settings>(settingsjson);

                _username = settings.Username ?? "Enter role";
                _serverAddress = new Uri(settings.ServerAddress ?? "http://localhost:13768/");
            }
            else
            {
                _username = "Enter role";
                _serverAddress = new Uri("http://localhost:13768/");
            }
            _status = "";
            _password = "Password";
        }

        #region Databindings

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                NotifyPropertyChanged("Username");
            }
        }


        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                NotifyPropertyChanged("Status");
            }
        }

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
            var connection = new ServerConnection(_serverAddress);
            try
            {
                var roles = await connection.Login(Username);
                Status = "Login successful";
                EventConnection.RoleForWorkflow = roles.RolesOnWorkflows;


                // Save settings
                var settings = new Settings
                {
                    ServerAddress = _serverAddress.AbsoluteUri,
                    Username = _username
                };
                File.WriteAllText("settings.json", JsonConvert.SerializeObject(settings, Formatting.Indented));
                // Save settings end.

                var window = new MainWindow();
                window.Show();
                CloseAction.Invoke();
            }
            catch (Exception ex)
            {
                if (ex is LoginFailedException || ex is ServerNotFoundException)
                {
                    _loginStarted = false;
                    Status = ex.Message;
                }
                else
                {
                    throw;
                }
            }
            
        }

        #endregion
    }
}
