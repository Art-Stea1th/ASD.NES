using System;
using System.IO;
using System.Reflection;
using System.Windows;
using Microsoft.Win32;

namespace ASD.NES.WPF.Services {

    using Core;

    internal static class OpenFileService {

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

            if (openFileDialog.ShowDialog() != true)
                return (null, null);

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