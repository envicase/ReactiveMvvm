using System;
using ReactiveMvvm.ViewModels;

namespace UserManager
{
    public class UserViewModel : ReactiveViewModel<User, int>
    {
        public UserViewModel(User user)
            : base(user)
        {
        }
    }
}
