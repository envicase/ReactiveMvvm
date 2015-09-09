using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReactiveMvvm
{
    // TODO: ObservableObject 클래스에 XML 주석이 작성되면 pragam 지시문을
    // 삭제해주세요.
#pragma warning disable 1591
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) =>
            PropertyChanged?.Invoke(this, e);

        protected void OnPropertyChanged(string propertyName) =>
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

        protected virtual bool Equals<T>(T left, T right) =>
            EqualityComparer<T>.Default.Equals(left, right);

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design",
            "CA1045:DoNotPassTypesByReference",
            MessageId = "0#",
            Justification =
                "This form is generally used to implement"
                + " observable properties")]
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
