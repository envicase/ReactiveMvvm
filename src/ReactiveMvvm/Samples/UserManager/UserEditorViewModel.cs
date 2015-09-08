using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveMvvm;
using ReactiveMvvm.ViewModels;

namespace UserManager
{
    public class UserEditorViewModel : ReactiveViewModel<User, int>
    {
        private string _editName;
        private string _editEmail;
        private ReactiveCommand<Unit> _editCommand;

        public UserEditorViewModel(int id)
            : base(id)
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

            EditCommand.RaiseCanExecuteChanged();
        }

        public ReactiveCommand<Unit> EditCommand
        {
            get
            {
                if (_editCommand == null)
                {
                    Func<object, bool> canExecute = p =>
                        Model != null &&
                        string.IsNullOrWhiteSpace(_editName) == false &&
                        string.IsNullOrWhiteSpace(_editEmail) == false &&
                        (_editName != Model.Name && _editEmail != Model.Email);

                    _editCommand = ReactiveCommand.Create(
                        Observable.Return(canExecute),
                        p =>
                        {
                            Stream.OnNext(Observable.Return(
                                new User(Id, _editName, _editEmail)));

                            return Task.FromResult(Unit.Default);
                        });
                }

                return _editCommand;
            }
        }
    }
}
