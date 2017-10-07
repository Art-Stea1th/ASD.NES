using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ASD.NES.WPF.ViewModels {

    using Core;
    using Helpers;
    using Services;
    using Controllers;

    internal sealed partial class ShellViewModel : Observable {

        public string Title => $"ASD.NES :: Shell - Status :: {console.State}";

        private Console console;
        private Cartridge cartridge;

        private WriteableBitmap screen;
        public ImageSource Screen => screen;

        public ICommand OpenFile => new RelayCommand(OpenFileCommandExecute);
        public ICommand Exit => new RelayCommand<Window>(w => w.Close());

        private Dispatcher dispatcher;

        public ShellViewModel() {
            dispatcher = Dispatcher.CurrentDispatcher;
            screen = new WriteableBitmap(256, 240, 96, 96, PixelFormats.Bgr32, null);
            console = new Console(new KeyboardController(dispatcher));
            console.NextFrameReady += UpdateScreen;
            InitializeControlCommands();
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
    }
}