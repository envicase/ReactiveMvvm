using System;

namespace ReactiveMvvm.Models
{
    internal sealed class DefaultCoalescer<T> : Coalescer<T>
        where T : class
    {
        public override T Coalesce(T left, T right)
        {
            if (left == null)
            {
                throw new ArgumentNullException(nameof(left));
            }
            if (right == null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            var coalescable = left as ICoalescable<T>;
            return coalescable == null ? left : coalescable.Coalesce(right);
        }
    }
}
