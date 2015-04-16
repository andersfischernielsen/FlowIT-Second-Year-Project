using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Client
{
    class Settings
    {
        public string ServerAddress { get; set; }
        public string Username { get; set; }

        public static Settings LoadSettings()
        {
            Settings settings;
            if (File.Exists("settings.json"))
            {
                var settingsjson = File.ReadAllText("settings.json");
                settings =  JsonConvert.DeserializeObject<Settings>(settingsjson);

                settings.Username = settings.Username ?? "Enter role";
                settings.ServerAddress = settings.ServerAddress ?? "http://flowit.azurewebsites.net/";
            }
            else
            {
                settings = new Settings { Username = "Enter role", ServerAddress = "http://flowit.azurewebsites.net/" };
            }
            return settings;
        }
    }
}
