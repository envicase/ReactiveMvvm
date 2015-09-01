using System;

namespace ReactiveMvvm.Models
{
    public interface ICoalescable<T>
        where T : class
    {
        T Coalese(T right);
    }
}
