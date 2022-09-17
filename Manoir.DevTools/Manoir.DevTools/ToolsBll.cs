using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Services.Maps;
    
namespace Manoir.DevTools
{
    public static class ToolsBll
    {
        public class Settings : ObservableCollection<SettingsEnvironnement>, INotifyPropertyChanged
        {
            #region les settings généraux

            private SettingsLocal _local = new SettingsLocal();
            public SettingsLocal Local
            {
                get { return _local; }
                set
                {
                    _local = value;
                    OnPropertyChanged("Local");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            private void OnPropertyChanged(string propertyName)
            {
                var evt = PropertyChanged;
                if (evt != null)
                    evt(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
        }

        public class SettingsLocal : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged(string propertyName)
            {
                var evt = PropertyChanged;
                if (evt != null)
                    evt(this, new PropertyChangedEventArgs(propertyName));
            }


            private string _rootRepo = null;
            public string RootForManoirRepo
            {
                get { return _rootRepo; }
                set
                {
                    _rootRepo = value;
                    OnPropertyChanged("RootForManoirRepo");
                }
            }

        }

        public class SettingsEnvironnement
        {
            public string Name { get; set; }
            public string ServerUrl { get; set; }
            public string DockerRepositoyUrl { get; set; }
            public string DockerTagForImages { get; set; }
        }

        public static Settings Load()
        {
            var pth = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "manoir.app", "devtools.settings.json");
            Settings settings = null;
            if (File.Exists(pth))
                settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(pth));
            else
                settings = new Settings();
            pth = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "manoir.app", "devtools.settings.local.json");
            if (File.Exists(pth))
                settings.Local = JsonSerializer.Deserialize<SettingsLocal>(File.ReadAllText(pth));
            else
                settings.Local = new SettingsLocal() { RootForManoirRepo = "c:\\test"};

            return settings;
        }

        public static void Save(Settings settings)
        {
            var pth = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "manoir.app");
            if (!Directory.Exists(pth))
                Directory.CreateDirectory(pth);
            pth = Path.Combine(pth, "devtools.settings.json");
            var tmp = JsonSerializer.Serialize(settings);
            Settings settingsTmp = JsonSerializer.Deserialize<Settings>(tmp);
            settingsTmp.Local = null;
            File.WriteAllText(pth, JsonSerializer.Serialize(settingsTmp));

            pth = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "manoir.app");
            if (!Directory.Exists(pth))
                Directory.CreateDirectory(pth);
            pth = Path.Combine(pth, "devtools.settings.local.json");
            File.WriteAllText(pth, JsonSerializer.Serialize(settings.Local));
        }

        public static void NewEnvironment(Settings settings)
        {
            SettingsEnvironnement env = new SettingsEnvironnement()
            {
                Name = "New"
            };
            settings.Add(env);
            Save(settings);
        }
    }
}
