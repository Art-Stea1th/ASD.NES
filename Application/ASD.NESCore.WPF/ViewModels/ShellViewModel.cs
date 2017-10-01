using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ASD.NESCore.WPF.ViewModels {

    using Helpers;
    using Services;

    internal sealed partial class ShellViewModel : Observable {

        public string Title => $"NESCore :: Shell - Status :: {console.State}";

        private Console console;
        private Cartridge cartridge;

        private WriteableBitmap screen;
        public ImageSource Screen => screen;

        public ICommand OpenFile => new RelayCommand(OpenFileCommandExecute);
        public ICommand Exit => new RelayCommand<Window>(w => w.Close());

        public ShellViewModel() {
            screen = new WriteableBitmap(256, 240, 96, 96, PixelFormats.Bgr32, null);
            console = new Console();
            InitializeControlCommands();
        }

        private void OpenFileCommandExecute() {
            cartridge = OpenFileService.OpenCartridgeFile();
            if (cartridge != null) {
                console = new Console(cartridge);
            }
        }
    }
}