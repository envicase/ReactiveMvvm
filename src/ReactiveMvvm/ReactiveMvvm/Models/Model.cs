using System;

namespace ReactiveMvvm.Models
{
    public abstract class Model<TId>
        where TId : IEquatable<TId>
    {
        protected Model(TId id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            Id = id;
        }

        public TId Id { get; }
    }
}
