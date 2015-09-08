using System;
using System.ComponentModel;
using System.Reactive;
using ReactiveMvvm;
using ReactiveMvvm.ViewModels;

namespace UserManager
{
    public class UserEditorViewModel : ReactiveViewModel<User, int>
    {
        private string _editName;
        private string _editEmail;
        private ReactiveCommand<Unit> _restoreCommand;
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

            _restoreCommand?.RaiseCanExecuteChanged();
            _saveCommand?.RaiseCanExecuteChanged();
        }

        private bool HasChanges =>
            EditName != Model.Name || EditEmail != Model.Email;

        public ReactiveCommand<Unit> RestoreCommand
        {
            get
            {
                if (_restoreCommand == null)
                {
                    _restoreCommand = ReactiveCommand.Create(
                        _ => HasChanges,
                        _ =>
                        {
                            EditName = Model.Name;
                            EditEmail = Model.Email;
                        });
                }

                return _restoreCommand;
            }
        }

        private bool HasValue(string s) => !string.IsNullOrWhiteSpace(s);

        public ReactiveCommand<Unit> SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = ReactiveCommand.Create(
                        _ =>
                            HasChanges &&
                            HasValue(EditName) &&
                            HasValue(EditEmail),
                        _ => Stream.OnNext(new User(Id, EditName, EditEmail)));
                }

                return _saveCommand;
            }
        }
    }
}
