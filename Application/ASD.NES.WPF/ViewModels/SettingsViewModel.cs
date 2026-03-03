using System;
using System.Windows;
using System.Windows.Input;

namespace ASD.NES.WPF.ViewModels {

    using Helpers;
    using Models;

    internal sealed class SettingsViewModel : Observable {

        private string capturingSlot;

        public string P1Left { get; set; }
        public string P1Up { get; set; }
        public string P1Right { get; set; }
        public string P1Down { get; set; }
        public string P1Select { get; set; }
        public string P1Start { get; set; }
        public string P1B { get; set; }
        public string P1A { get; set; }

        public string P2Left { get; set; }
        public string P2Up { get; set; }
        public string P2Right { get; set; }
        public string P2Down { get; set; }
        public string P2Select { get; set; }
        public string P2Start { get; set; }
        public string P2B { get; set; }
        public string P2A { get; set; }

        public string CapturingSlot {
            get => capturingSlot;
            set {
                Set(ref capturingSlot, value);
                CapturingHint = string.IsNullOrEmpty(value) ? "" : "Press key for: " + value;
                OnPropertyChanged(nameof(CapturingHint));
            }
        }

        public string CapturingHint { get; private set; }

        public ICommand CaptureKey => new RelayCommand<string>(slot => CapturingSlot = slot);
        public ICommand SaveAndClose => new RelayCommand<Window>(SaveAndCloseExecute);
        public ICommand Cancel => new RelayCommand<Window>(w => { if (w != null) { w.DialogResult = false; w.Close(); } });

        public SettingsViewModel() {
            LoadFromSettings(AppSettingsModel.Load());
        }

        public void LoadFromSettings(AppSettingsModel settings) {
            if (settings == null) {
                settings = AppSettingsModel.GetDefaults();
            }
            ApplyModel(settings.Player1, true);
            ApplyModel(settings.Player2, false);
        }

        private void ApplyModel(ControllerMappingModel m, bool player1) {
            if (m == null) {
                return;
            }
            if (player1) {
                P1Left = m.Left;
                P1Up = m.Up;
                P1Right = m.Right;
                P1Down = m.Down;
                P1Select = m.Select;
                P1Start = m.Start;
                P1B = m.B;
                P1A = m.A;
            }
            else {
                P2Left = m.Left;
                P2Up = m.Up;
                P2Right = m.Right;
                P2Down = m.Down;
                P2Select = m.Select;
                P2Start = m.Start;
                P2B = m.B;
                P2A = m.A;
            }
            OnPropertyChanged(null);
        }

        public void SetCapturedKey(Key key) {
            if (string.IsNullOrEmpty(CapturingSlot)) {
                return;
            }
            var keyStr = key.ToString();
            var slot = CapturingSlot;
            CapturingSlot = null;
            if (slot.StartsWith("P1_")) {
                SetP1(slot.Substring(3), keyStr);
            }
            else if (slot.StartsWith("P2_")) {
                SetP2(slot.Substring(3), keyStr);
            }
        }

        private void SetP1(string button, string value) {
            switch (button) {
                case "Left": P1Left = value; break;
                case "Up": P1Up = value; break;
                case "Right": P1Right = value; break;
                case "Down": P1Down = value; break;
                case "Select": P1Select = value; break;
                case "Start": P1Start = value; break;
                case "B": P1B = value; break;
                case "A": P1A = value; break;
            }
            OnPropertyChanged(null);
        }

        private void SetP2(string button, string value) {
            switch (button) {
                case "Left": P2Left = value; break;
                case "Up": P2Up = value; break;
                case "Right": P2Right = value; break;
                case "Down": P2Down = value; break;
                case "Select": P2Select = value; break;
                case "Start": P2Start = value; break;
                case "B": P2B = value; break;
                case "A": P2A = value; break;
            }
            OnPropertyChanged(null);
        }

        public AppSettingsModel BuildSettings() {
            return new AppSettingsModel {
                Player1 = new ControllerMappingModel {
                    Left = P1Left, Up = P1Up, Right = P1Right, Down = P1Down,
                    Select = P1Select, Start = P1Start, B = P1B, A = P1A
                },
                Player2 = new ControllerMappingModel {
                    Left = P2Left, Up = P2Up, Right = P2Right, Down = P2Down,
                    Select = P2Select, Start = P2Start, B = P2B, A = P2A
                }
            };
        }

        private void SaveAndCloseExecute(Window window) {
            var settings = BuildSettings();
            settings.Save();
            if (window != null) {
                window.DialogResult = true;
                window.Close();
            }
        }
    }
}
