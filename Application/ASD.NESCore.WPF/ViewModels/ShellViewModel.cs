using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ASD.NESCore.WPF.ViewModels {

    using Helpers;

    internal sealed class ShellViewModel : Observable {

        private WriteableBitmap screen;
        public ImageSource Screen => screen;

        public ShellViewModel() {
            screen = new WriteableBitmap(256, 240, 96, 96, PixelFormats.Bgr32, null);
        }
    }
}