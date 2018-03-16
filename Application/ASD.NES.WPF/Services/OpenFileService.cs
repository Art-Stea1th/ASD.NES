﻿using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace ASD.NES.WPF.Services {

    using Core;

    internal static class OpenFileService {

        public static Cartridge OpenCartridgeFile() {

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

            if (openFileDialog.ShowDialog() == true) {
                return Cartridge.Create(File.ReadAllBytes(openFileDialog.FileName));
            }
            return null;
        }
    }
}