using System.IO;
using Microsoft.Win32;

namespace ASD.NESCore.WPF.Services {

    internal static class OpenFileService {

        public static Cartridge OpenCartridgeFile() {

            var openFileDialog = new OpenFileDialog() {
                Title = "Open .NES file",
                Multiselect = false,
                DefaultExt = "*.nes",
                Filter = " iNES, NES 2.0 (*.nes) |*.nes;",
                ValidateNames = true
            };

            if (openFileDialog.ShowDialog() == true) {
                return Cartridge.Create(File.ReadAllBytes(openFileDialog.FileName));
            }
            return null;
        }
    }
}