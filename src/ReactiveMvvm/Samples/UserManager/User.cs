using System;
using ReactiveMvvm;

namespace UserManager
{
    public class User : Model<int>
    {
        public User(int id, string name, string email)
            : base(id)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (email == null)
            {
                throw new ArgumentNullException(nameof(email));
            }

            Name = name;
            Email = email;
        }

        public string Name { get; }

        public string Email { get; }
    }
}
