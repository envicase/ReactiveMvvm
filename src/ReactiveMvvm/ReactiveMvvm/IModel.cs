using System;

namespace ReactiveMvvm
{
    public interface IModel<TId>
        where TId : IEquatable<TId>
    {
        TId Id { get; }
    }
}
