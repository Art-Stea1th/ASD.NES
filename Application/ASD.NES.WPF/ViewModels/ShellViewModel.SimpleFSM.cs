using System;
using System.Windows.Input;

namespace ASD.NES.WPF.ViewModels {

    using Core;
    using Helpers;

    internal sealed partial class ShellViewModel {

        private RelayCommand turnOnCommand, turnOffCommand, pauseCommand, resumeCommand;
        private RelayCommand turnOnOffCommand, resumePauseCommand, resetCommand;

        public ICommand TurnOnOff {
            get => turnOnOffCommand;
            set => Set(ref turnOnOffCommand, (RelayCommand)value);
        }
        public ICommand PauseResume {
            get => resumePauseCommand;
            set => Set(ref resumePauseCommand, (RelayCommand)value);
        }
        public ICommand Reset {
            get => resetCommand;
            set => Set(ref resetCommand, (RelayCommand)value);
        }

        private void InitializeControlCommands() {
            turnOnCommand = new RelayCommand(
                () => ChangeControlState(() => console.PowerOn(), turnOffCommand, pauseCommand),
                () => console.State == State.Off, "Turn On");
            pauseCommand = new RelayCommand(
                () => ChangeControlState(() => console.Pause(), turnOffCommand, resumeCommand),
                () => console.State == State.On, "Pause");
            resumeCommand = new RelayCommand(
                () => ChangeControlState(() => console.Resume(), turnOffCommand, pauseCommand),
                () => console.State == State.Paused, "Resume");
            resetCommand = new RelayCommand(
                () => ChangeControlState(() => console.Reset(), turnOffCommand, pauseCommand),
                () => console.State == State.On || console.State == State.Paused, "Reset");
            turnOffCommand = new RelayCommand(
                () => ChangeControlState(() => console.PowerOff(), turnOnCommand, pauseCommand),
                () => console.State == State.On || console.State == State.Paused, "Turn Off");
            ChangeControlState(null, turnOnCommand, pauseCommand);
        }

        private void ChangeControlState(Action action, RelayCommand newOnOffCommand, RelayCommand newPauseResumeCommand) {
            action?.Invoke();
            TurnOnOff = newOnOffCommand;
            PauseResume = newPauseResumeCommand;
            OnPropertyChanged(nameof(Title));
        }
    }
}