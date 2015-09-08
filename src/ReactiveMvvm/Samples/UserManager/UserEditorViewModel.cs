using System;
using System.ComponentModel;
using System.Reactive;
using ReactiveMvvm;

namespace UserManager
{
    public class UserEditorViewModel : UserViewModel
    {
        private string _editName;
        private string _editEmail;
        private ReactiveCommand<Unit> _saveCommand;

        public UserEditorViewModel(User user)
            : base(user)
        {
        }

        public string EditName
        {
            get { return _editName; }
            set { SetValue(ref _editName, value); }
        }

        public string EditEmail
        {
            get { return _editEmail; }
            set { SetValue(ref _editEmail, value); }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(Model))
            {
                EditName = Model.Name;
                EditEmail = Model.Email;
            }

            SaveCommand.RaiseCanExecuteChanged();
        }

        private bool HasValue(string s) => !string.IsNullOrWhiteSpace(s);

        public ReactiveCommand<Unit> SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = ReactiveCommand.Create(
                        p => HasValue(_editName) && HasValue(_editEmail),
                        p => Stream.OnNext(
                            new User(Id, _editName, _editEmail)));
                }

                return _saveCommand;
            }
        }
    }
}
