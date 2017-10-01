using System;
using System.Windows;

namespace ASD.NESCore.WPF {

    public partial class App : Application {

        public App() => DispatcherUnhandledException += (sender, e) => {
            ShowExceptionMessage(e.Exception);
            e.Handled = true;
        };

        private void ShowExceptionMessage(Exception exception, bool showStackTrace = false) {
            var stackTrace = $"{Environment.NewLine}{Environment.NewLine}{exception.StackTrace}";
            var message = $"{exception.Message}" + (showStackTrace ? stackTrace : string.Empty);
            var title = $"{exception.GetType().Name}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    }
}