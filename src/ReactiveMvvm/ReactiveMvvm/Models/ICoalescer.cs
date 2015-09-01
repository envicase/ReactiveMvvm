using System;

namespace ReactiveMvvm.Models
{
    public interface ICoalescer<T>
        where T : class
    {
        T Coalese(T left, T right);
    }
}
