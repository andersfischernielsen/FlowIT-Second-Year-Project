using System.IO;
using Newtonsoft.Json;

namespace Client
{
    class Settings
    {
        public string ServerAddress { get; set; }

        public static Settings LoadSettings()
        {
            if (!File.Exists("settings.json"))
            {
                return new Settings { ServerAddress = "http://flowit.azurewebsites.net/" };
            }

            var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));

            settings.ServerAddress = settings.ServerAddress ?? "http://flowit.azurewebsites.net/";
            return settings;
        }

        public void SaveSettings()
        {
            File.WriteAllText("settings.json", JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
