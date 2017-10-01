using System;
using System.Windows.Input;


namespace ASD.NESCore.WPF.Helpers {

    public class RelayCommand : ICommand {

        private readonly Action execute;
        private readonly Func<bool> canExecute;

        public string Name { get; }

        public event EventHandler CanExecuteChanged {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action execute, Func<bool> canExecute = null, string name = null) {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
            Name = name;
        }

        public override string ToString() => Name;

        public bool CanExecute(object parameter = null) => canExecute == null || canExecute();
        public void Execute(object parameter = null) => execute();
    }

    public class RelayCommand<T> : ICommand {

        private readonly Action<T> execute;
        private readonly Func<T, bool> canExecute;

        public string Name { get; }

        public event EventHandler CanExecuteChanged {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null, string name = null) {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
            Name = name;
        }

        public bool CanExecute(object parameter) => canExecute == null || canExecute((T)parameter);
        public void Execute(object parameter) => execute((T)parameter);
    }
}