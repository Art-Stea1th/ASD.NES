using System;
using System.Windows;
using System.Windows.Threading;

namespace ASD.NES.WPF {

    using ViewModels;
    using Views;

    public partial class App : Application {

        internal IDisposable DisposableViewModel { get; private set; }

        public App() {

            Startup += (sender, e) => {
                DisposableViewModel = new ShellViewModel();
                MainWindow = new ShellView { DataContext = DisposableViewModel };
                MainWindow.Show();

                var nesPath = Core.StartupArgsHelper.GetFirstNesPath(Environment.GetCommandLineArgs());
                if (!string.IsNullOrEmpty(nesPath)) {
                    Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(() => {
                        if (MainWindow?.DataContext is ShellViewModel vm)
                            vm.LoadRomFromPath(nesPath);
                    }));
                }
            };

            Exit += (sender, e) => DisposableViewModel?.Dispose();

            DispatcherUnhandledException += (sender, e) => {
                ShowExceptionMessage(e.Exception, true);
                e.Handled = true;
            };
        }

        private void ShowExceptionMessage(Exception exception, bool showStackTrace = false) {
            var stackTrace = $"{Environment.NewLine}{Environment.NewLine}{exception.StackTrace}";
            var message = $"{exception.Message}" + (showStackTrace ? stackTrace : string.Empty);
            var title = $"{exception.GetType().Name}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    }
}