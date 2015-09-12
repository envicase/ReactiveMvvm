using System;
using System.Reactive.Linq;
using ReactiveMvvm;
using ReactiveMvvm.ViewModels;

namespace UserManager
{
    public class UserEditorViewModel : ReactiveViewModel<User, int>
    {
        private string _editName;
        private string _editEmail;

        public UserEditorViewModel(User user)
            : base(user)
        {
            this.Observe(x => x.Model).Subscribe(_ => ProjectModel());
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

        private void ProjectModel()
        {
            EditName = Model.Name;
            EditEmail = Model.Email;
        }

        private IObservable<bool> HasChanges => Observable.CombineLatest(
            this.Observe(c => c.EditName, p => p != Model.Name),
            this.Observe(c => c.EditEmail, p => p != Model.Email),
            CombineOperators.Or);

        public IReactiveCommand RestoreCommand => ReactiveCommand.Create(
            HasChanges.ObserveOnDispatcher(), _ => ProjectModel());

        private bool HasValue(string s) => !string.IsNullOrWhiteSpace(s);

        public IReactiveCommand SaveCommand => ReactiveCommand.Create(
            HasChanges.CombineLatest(
                this.Observe(c => c.EditName, HasValue),
                this.Observe(c => c.EditEmail, HasValue),
                CombineOperators.And).ObserveOnDispatcher(),
            _ => Stream.OnNext(new User(Id, EditName, EditEmail)));
    }
}
