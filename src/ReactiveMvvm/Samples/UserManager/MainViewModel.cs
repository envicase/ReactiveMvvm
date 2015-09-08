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
            set
            {
                if (SetValue(ref _selectedItem, value))
                {
                    OnSelectedItemChanged();
                }
            }
        }

        public Visibility EditorVisibility =>
            _selectedItem == null ? Visibility.Hidden : Visibility.Visible;

        public Visibility GuideMessageVisibility =>
            _selectedItem == null ? Visibility.Visible : Visibility.Hidden;

        private void OnSelectedItemChanged()
        {
            Editor = _selectedItem == null ?
                null : new UserEditorViewModel(_selectedItem.Model);
            OnPropertyChanged(nameof(EditorVisibility));
            OnPropertyChanged(nameof(GuideMessageVisibility));
        }
    }
}
