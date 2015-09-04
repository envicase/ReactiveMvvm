using System;

namespace ReactiveMvvm
{
    public interface ICoalescable<T>
        where T : class
    {
        T Coalesce(T right);
    }
}
