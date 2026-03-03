using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using NAudio.Wave;

namespace ASD.NES.WPF.ViewModels {

    using Controllers;
    using Core;
    using DataProviders;
    using Helpers;
    using Models;
    using Services;
    using Views;

    internal sealed partial class ShellViewModel : Observable {

        public string Title => $"ASD.NES :: Shell - Status :: {console.State}";

        private Console console;
        private Cartridge cartridge;

        private WriteableBitmap screen;
        private WaveOut audioDevice;

        public ImageSource Screen => screen;

        public ICommand OpenFile => new RelayCommand(OpenFileCommandExecute);
        public ICommand Exit => new RelayCommand<Window>(w => w.Close());
        public ICommand ViewHelp => new RelayCommand(
            () => Process.Start("https://github.com/Art-Stea1th/ASD.NES/blob/master/README.md"),
            () => true, "View Help (Online)");
        public ICommand OpenSettings => new RelayCommand(OpenSettingsExecute, () => true, "Settings...");

        private Dispatcher dispatcher;

        public ShellViewModel() {
            dispatcher = Dispatcher.CurrentDispatcher;
            screen = new WriteableBitmap(256, 240, 96, 96, PixelFormats.Bgr32, null);
            console = new Console();
            ConfigureAudioDevice();
            ConfigureControllers();
            console.NextFrameReady += UpdateScreen;
            console.PlayAudio += audioDevice.Play;
            InitializeControlCommands();
        }

        private void ConfigureAudioDevice() {
            var waveProvider = new WaveProvider(console.AudioBuffer);
            audioDevice = new WaveOut();
            audioDevice.DesiredLatency = 50;  // ms; lower = less input-to-sound delay (default 300)
            audioDevice.NumberOfBuffers = 2;
            audioDevice.Init(waveProvider);
        }

        private void ConfigureControllers() {
            var settings = AppSettingsModel.Load();
            settings.Player1.ToKeys(out var p1L, out var p1U, out var p1R, out var p1D, out var p1Sel, out var p1St, out var p1B, out var p1A);
            settings.Player2.ToKeys(out var p2L, out var p2U, out var p2R, out var p2D, out var p2Sel, out var p2St, out var p2B, out var p2A);
            console.PlayerOneController = new KeyboardController(dispatcher) {
                Left = p1L, Up = p1U, Right = p1R, Down = p1D,
                Select = p1Sel, Start = p1St, B = p1B, A = p1A
            };
            console.PlayerTwoController = new KeyboardController(dispatcher) {
                Left = p2L, Up = p2U, Right = p2R, Down = p2D,
                Select = p2Sel, Start = p2St, B = p2B, A = p2A
            };
        }

        private void OpenSettingsExecute() {
            var settingsWindow = new SettingsView {
                Owner = Application.Current.MainWindow
            };
            if (settingsWindow.ShowDialog() == true) {
                ConfigureControllers();
            }
        }

        private void OpenFileCommandExecute() {
            var (loadedCartridge, filePath) = OpenFileService.OpenCartridgeFile();
            if (loadedCartridge != null) {
                cartridge = loadedCartridge;
                var oldConsole = console;
                oldConsole.PowerOff();
                console.NextFrameReady -= UpdateScreen;
                console.PlayAudio -= audioDevice.Play;
                oldConsole.Dispose();

                console = new Console();
                ConfigureAudioDevice();
                ConfigureControllers();
                console.NextFrameReady += UpdateScreen;
                console.PlayAudio += audioDevice.Play;
                // Prefer PAL for European dumps "(E)" that often lack the PAL flag in iNES header
                var fileName = filePath != null ? Path.GetFileName(filePath) : "";
                console.PreferRegion = (fileName.IndexOf("(E)", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        fileName.IndexOf("(Europe)", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        fileName.IndexOf("(PAL)", StringComparison.OrdinalIgnoreCase) >= 0)
                    ? TvRegion.PAL
                    : (TvRegion?)null;
                console.InsertCartridge(cartridge);
                (Reset as RelayCommand).Execute();
            }
        }

        private void UpdateScreen(uint[] data) {
            if (dispatcher.HasShutdownStarted) {
                return;
            }
            try {
                dispatcher.Invoke(()
                    => screen.WritePixels(new Int32Rect(0, 0, 256, 240), data, 256 * sizeof(uint), 0));
                DebugLogFrameIfEnabled();
            } catch (OperationCanceledException) { /* shutdown: dispatcher canceled the invoke */ }
        }

        private static int _debugFrameCount;
        private static void DebugLogFrameIfEnabled() {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASD_NES_DEBUG"))) {
                return;
            }
            _debugFrameCount++;
            if (_debugFrameCount % 300 != 0) {
                return;
            }
            try {
                var path = Path.Combine(Path.GetTempPath(), "ASD_NES_debug_frames.txt");
                File.AppendAllText(path, $"{DateTime.UtcNow:O} frames={_debugFrameCount}{Environment.NewLine}");
            } catch (Exception) { /* ignore */ }
        }

        protected override void OnDispose() {
            try {
                if (console != null) {
                    console.PowerOff();
                    console.Dispose();
                    console = null;
                }
            } catch (Exception) { /* avoid crash on close */ }
        }
    }
}