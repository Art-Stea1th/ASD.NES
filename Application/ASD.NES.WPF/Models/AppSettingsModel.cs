using System;
using System.IO;
using System.Windows.Input;
using System.Xml.Serialization;

namespace ASD.NES.WPF.Models {

    public sealed class AppSettingsModel {

        public ControllerMappingModel Player1 { get; set; }
        public ControllerMappingModel Player2 { get; set; }

        private static string SettingsPath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ASD.NES.Settings.xml");

        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(AppSettingsModel));

        public static AppSettingsModel Load() {
            try {
                var path = SettingsPath;
                if (!File.Exists(path)) {
                    return GetDefaults();
                }
                using (var fs = File.OpenRead(path)) {
                    var loaded = (AppSettingsModel)Serializer.Deserialize(fs);
                    if (loaded?.Player1 != null && loaded.Player2 != null) {
                        return loaded;
                    }
                }
            }
            catch (Exception) {
                // use defaults
            }
            return GetDefaults();
        }

        public void Save() {
            try {
                var path = SettingsPath;
                using (var fs = File.Create(path)) {
                    Serializer.Serialize(fs, this);
                }
            }
            catch (Exception) {
                // ignore
            }
        }

        public static AppSettingsModel GetDefaults() {
            return new AppSettingsModel {
                Player1 = ControllerMappingModel.FromKeys(
                    Key.A, Key.W, Key.D, Key.S,
                    Key.LeftShift, Key.Enter, Key.K, Key.L),
                Player2 = ControllerMappingModel.FromKeys(
                    Key.Left, Key.Up, Key.Right, Key.Down,
                    Key.RightShift, Key.Enter, Key.Insert, Key.Delete)
            };
        }
    }
}
