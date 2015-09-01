using System;

namespace ReactiveMvvm.Models
{
    public abstract class Coalescer<T> : ICoalescer<T>
        where T : class
    {
        public static Coalescer<T> Default { get; }

        static Coalescer()
        {
            Default = new DefaultCoalescer<T>();
        }

        public abstract T Coalesce(T left, T right);
    }
}
