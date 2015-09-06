using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReactiveMvvm
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) =>
            PropertyChanged?.Invoke(this, e);

        protected void OnPropertyChanged(string propertyName) =>
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

        protected virtual bool Equals<T>(T x, T y) =>
            EqualityComparer<T>.Default.Equals(x, y);

        protected bool SetValue<T>(
            ref T store, T value, [CallerMemberName]string propertyName = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }
            if (Equals(store, value))
            {
                return false;
            }

            store = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
