using System;

namespace ReactiveMvvm.Tests
{
    public class UserCoalescable : User, ICoalescable<UserCoalescable>
    {
        public UserCoalescable(string id, string userName, string bio)
            : base(id, userName, bio)
        {
        }

        public UserCoalescable Coalesce(UserCoalescable right)
        {
            if (right == null)
            {
                throw new ArgumentNullException(nameof(right));
            }
            if (right.Id != Id)
            {
                var message = $"{nameof(right)}.{nameof(right.Id)} is invalid.";
                throw new ArgumentException(message, nameof(right));
            }

            if (UserName != null && Bio != null)
            {
                return this;
            }

            return new UserCoalescable(
                Id, UserName ?? right.UserName, Bio ?? right.Bio);
        }
    }
}
