using System.IO;
using Newtonsoft.Json;

namespace Client
{
    class Settings
    {
        public string ServerAddress { get; set; }
        public string Username { get; set; }

        public string Password { get; set; }

        public static Settings LoadSettings()
        {
            Settings settings;
            if (File.Exists("settings.json"))
            {
                var settingsjson = File.ReadAllText("settings.json");
                settings =  JsonConvert.DeserializeObject<Settings>(settingsjson);

                settings.Username = settings.Username ?? "Enter role";
                settings.Password = settings.Password ?? "Password";
                settings.ServerAddress = settings.ServerAddress ?? "http://flowit.azurewebsites.net/";
            }
            else
            {
                settings = new Settings { Username = "Enter role", Password = "Password", ServerAddress = "http://flowit.azurewebsites.net/" };
            }
            return settings;
        }
    }
}
