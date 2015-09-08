using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveMvvm;

namespace UserManager
{
    public class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            var users = new User[]
            {
                new User(1, "Tony Stark", "ironman@avengers.com"),
                new User(2, "Bruce Banner", "hulk@avengers.com"),
                new User(3, "Thor Odinson", "thor@avengers.com"),
                new User(4, "Steve Rogers", "captain@avengers.com"),
            };
            Users = (from u in users
                     select new UserItemViewModel(u)).ToList();
        }

        public IEnumerable<UserItemViewModel> Users { get; }
    }
}
