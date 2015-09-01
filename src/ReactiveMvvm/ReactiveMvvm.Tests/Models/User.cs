using System;
using ReactiveMvvm.Models;

namespace ReactiveMvvm.Tests.Models
{
    public class User : Model<string>
    {
        public User(string id, string userName, string bio)
            : base(id)
        {
            if (userName == null)
            {
                throw new ArgumentNullException(nameof(userName));
            }

            UserName = userName;
            Bio = bio;
        }

        public string UserName { get; }

        public string Bio { get; }
    }
}
