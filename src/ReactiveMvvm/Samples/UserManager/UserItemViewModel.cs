using System;
using System.Reactive.Linq;
using ReactiveMvvm.ViewModels;

namespace UserManager
{
    public class UserItemViewModel : ReactiveViewModel<User, int>
    {
        private static T ShouldNotBeNull<T>(T value, string paramName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }

            return value;
        }

        public UserItemViewModel(User user)
            : base(ShouldNotBeNull(user, nameof(user)).Id)
        {
            Stream.OnNext(Observable.Return(user));
        }

        public UserItemViewModel(int id)
            : base(id)
        {
        }
    }
}
