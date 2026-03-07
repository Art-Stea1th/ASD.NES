using System;
using System.IO;
using System.Reflection;
using System.Windows;
using Microsoft.Win32;

namespace ASD.NES.WPF.Services {

    using Core;

    internal static class OpenFileService {

        /// <summary> Load cartridge from path without dialog (e.g. startup with .nes path). Returns (cartridge, filePath) or (null, null) on error. </summary>
        public static (Cartridge cartridge, string filePath) LoadCartridgeFromPath(string path) {
            if (string.IsNullOrWhiteSpace(path) || !path.EndsWith(".nes", StringComparison.OrdinalIgnoreCase)) {
                return (null, null);
            }
            try {
                if (!System.IO.File.Exists(path))
                    return (null, null);
                var data = File.ReadAllBytes(path);
                return (Cartridge.Create(data), path);
            } catch {
                return (null, null);
            }
        }

        /// <summary> Returns (cartridge, filePath) or (null, null) on cancel/error. File path is used to detect PAL from "(E)" etc. </summary>
        public static (Cartridge cartridge, string filePath) OpenCartridgeFile() {
            var openFileDialog = new OpenFileDialog() {
                Title = "Open .NES file",
                Multiselect = false,
                DefaultExt = "*.nes",
                Filter = " iNES, NES 2.0 (*.nes) |*.nes;",
                ValidateNames = true
            };

            string GetExecutionAssemblyPath()
                => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var gamesFullPath = $"{GetExecutionAssemblyPath()}\\Games\\";

            if (Directory.Exists(gamesFullPath)) {
                openFileDialog.InitialDirectory = gamesFullPath;
            }

            if (openFileDialog.ShowDialog() != true) {
                return (null, null);
            }

            try {
                var data = File.ReadAllBytes(openFileDialog.FileName);
                return (Cartridge.Create(data), openFileDialog.FileName);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Cannot load ROM", MessageBoxButton.OK, MessageBoxImage.Warning);
                return (null, null);
            }
        }
    }
}