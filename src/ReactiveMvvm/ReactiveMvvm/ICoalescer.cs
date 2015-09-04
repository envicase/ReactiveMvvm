using System;

namespace ReactiveMvvm
{
    public interface ICoalescer<T>
        where T : class
    {
        T Coalesce(T left, T right);
    }
}
