using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using NAudio.Wave;

namespace ASD.NES.WPF.ViewModels {

    using Controllers;
    using DataProviders;
    using Helpers;
    using Core;
    using Services;

    internal sealed partial class ShellViewModel : Observable {

        public string Title => $"ASD.NES :: Shell - Status :: {console.State}";

        private Console console;
        private Cartridge cartridge;

        private WriteableBitmap screen;
        private WaveOut audioDevice;

        public ImageSource Screen => screen;

        public ICommand OpenFile => new RelayCommand(OpenFileCommandExecute);
        public ICommand Exit => new RelayCommand<Window>(w => w.Close());

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
            audioDevice.Init(waveProvider);
        }

        private void ConfigureControllers() { // TODO: move to settings ViewModel

            console.PlayerOneController = new KeyboardController(dispatcher) {
                Left = Key.A, Up = Key.W, Right = Key.D, Down = Key.S,
                Select = Key.RightShift, Start = Key.Enter, B = Key.K, A = Key.L
            };
            console.PlayerTwoController = new KeyboardController(dispatcher) {
                Left = Key.Left, Up = Key.Up, Right = Key.Right, Down = Key.Down,
                Select = Key.RightShift, Start = Key.Enter, B = Key.Insert, A = Key.Delete
            };
        }

        private void OpenFileCommandExecute() {
            cartridge = OpenFileService.OpenCartridgeFile();
            if (cartridge != null) {
                console.InsertCartridge(cartridge);
                (Reset as RelayCommand).Execute();
            }
        }

        private void UpdateScreen(uint[] data)
            => dispatcher.Invoke(()
                => screen.WritePixels(new Int32Rect(0, 0, 256, 240), data, 256 * sizeof(uint), 0));

        protected override void OnDispose() => console?.Dispose();
    }
}