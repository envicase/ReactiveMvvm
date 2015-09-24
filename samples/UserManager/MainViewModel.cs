using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ReactiveMvvm;

namespace UserManager
{
    public class MainViewModel : ObservableObject
    {
        public static IEnumerable<User> GetSampleData() => new[]
        {
            new User(1, "Tony Stark", "ironman@avengers.com"),
            new User(2, "Bruce Banner", "hulk@avengers.com"),
            new User(3, "Thor Odinson", "thor@avengers.com"),
            new User(4, "Steve Rogers", "captain@avengers.com"),
        };

        private UserViewModel _selectedItem;
        private UserEditorViewModel _editor;

        public MainViewModel()
        {
            Users = (from u in GetSampleData()
                     select new UserViewModel(u)).ToList();

            this.Observe(c => c.SelectedItem)
                .Subscribe(i => Editor = i == null ? null :
                                new UserEditorViewModel(i.Model));

            this.Observe(c => c.Editor)
                .Subscribe(_ =>
                {
                    OnPropertyChanged(nameof(EditorVisibility));
                    OnPropertyChanged(nameof(GuideMessageVisibility));
                });
        }

        public IEnumerable<UserViewModel> Users { get; }

        public UserEditorViewModel Editor
        {
            get { return _editor; }
            private set { SetValue(ref _editor, value); }
        }

        public UserViewModel SelectedItem
        {
            get { return _selectedItem; }
            set { SetValue(ref _selectedItem, value); }
        }

        public Visibility EditorVisibility =>
            _selectedItem == null ? Visibility.Hidden : Visibility.Visible;

        public Visibility GuideMessageVisibility =>
            _selectedItem == null ? Visibility.Visible : Visibility.Hidden;
    }
}
