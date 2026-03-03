using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace ASD.NES.WPF.Views {

    using ViewModels;

    public partial class SettingsView : Window {

        public SettingsView() {
            InitializeComponent();
            var vm = new SettingsViewModel();
            DataContext = vm;
            vm.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(SettingsViewModel.CapturingSlot) && DataContext is SettingsViewModel vm && !string.IsNullOrEmpty(vm.CapturingSlot)) {
                Focus();
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (!(DataContext is SettingsViewModel vm) || string.IsNullOrEmpty(vm.CapturingSlot)) {
                return;
            }
            if (e.Key == Key.System) {
                return;
            }
            e.Handled = true;
            vm.SetCapturedKey(e.Key);
        }
    }
}
