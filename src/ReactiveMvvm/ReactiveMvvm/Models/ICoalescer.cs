using System;

namespace ReactiveMvvm.Models
{
    public interface ICoalescer<T>
        where T : class
    {
        T Coalesce(T left, T right);
    }
}
