using System;
using System.Reactive.Linq;
using ReactiveMvvm.ViewModels;

namespace UserManager
{
    public class UserItemViewModel : ReactiveViewModel<User, int>
    {
        private static T NullReferenceGuard<T>(T value, string paramName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }

            return value;
        }

        public UserItemViewModel(User user)
            : base(NullReferenceGuard(user, nameof(user)).Id)
        {
            Stream.OnNext(Observable.Return(user));
        }
    }
}
