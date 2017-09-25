using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ASD.NESCore.WPF.Helpers {

    public abstract class Observable : INotifyPropertyChanged, IDisposable {

        public event PropertyChangedEventHandler PropertyChanged;

        protected Observable() { }

        protected void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null) {
            if (Equals(storage, value)) { return; }
            storage = value; OnPropertyChanged(propertyName);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public void Dispose() => OnDispose();
        protected virtual void OnDispose() { }
    }
}