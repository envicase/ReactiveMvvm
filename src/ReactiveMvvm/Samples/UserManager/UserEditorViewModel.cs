using System;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveMvvm;
using ReactiveMvvm.ViewModels;

namespace UserManager
{
    public class UserEditorViewModel : ReactiveViewModel<User, int>
    {
        private string _editName;
        private string _editEmail;
        private IObservable<bool> _canExecuteRestoreCommand;
        private ReactiveCommand<Unit> _restoreCommand;
        private IObservable<bool> _canExecuteSaveCommand;
        private ReactiveCommand<Unit> _saveCommand;

        public UserEditorViewModel(User user)
            : base(user)
        {
            this.Observe(x => x.Model).Subscribe(_ => ProjectModel());

            var hasChanges = Observable.CombineLatest(
                this.Observe(c => c.EditName, p => p != Model.Name),
                this.Observe(c => c.EditEmail, p => p != Model.Email),
                CombineOperators.Or);

            _canExecuteRestoreCommand = hasChanges.ObserveOnDispatcher();

            _canExecuteSaveCommand = hasChanges.CombineLatest(
                this.Observe(c => c.EditName, HasValue),
                this.Observe(c => c.EditEmail, HasValue),
                CombineOperators.And).ObserveOnDispatcher();
        }

        private bool HasValue(string s) => !string.IsNullOrWhiteSpace(s);

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

        private void ProjectModel()
        {
            EditName = Model.Name;
            EditEmail = Model.Email;
        }

        public ReactiveCommand<Unit> RestoreCommand
        {
            get
            {
                if (_restoreCommand == null)
                {
                    _restoreCommand = ReactiveCommand.Create(
                        _canExecuteRestoreCommand, _ => ProjectModel());
                }

                return _restoreCommand;
            }
        }

        public ReactiveCommand<Unit> SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = ReactiveCommand.Create(
                        _canExecuteSaveCommand,
                        _ => Stream.OnNext(new User(Id, EditName, EditEmail)));
                }

                return _saveCommand;
            }
        }
    }
}
