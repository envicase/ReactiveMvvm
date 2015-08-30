using System;
using System.Collections.Generic;

namespace ReactiveMvvm.Tests.Models
{
    public class UserEqualityComparer : IEqualityComparer<User>
    {
        public bool Equals(User x, User y)
        {
            if (x == y)
            {
                return true;
            }
            if (x == null ^ y == null)
            {
                return false;
            }

            return x.Id == y.Id
                && x.UserName == y.UserName
                && x.Bio == y.Bio;
        }

        public int GetHashCode(User user)
        {
            return user.GetHashCode();
        }
    }
}
