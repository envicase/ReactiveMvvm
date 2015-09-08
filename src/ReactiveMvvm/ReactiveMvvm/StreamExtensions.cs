using System;
using System.Reactive.Linq;

namespace ReactiveMvvm
{
    public static class StreamExtensions
    {
        public static void OnNext<TModel, TId>(
            this Stream<TModel, TId> stream, TModel value)
            where TModel : class, IModel<TId>
            where TId : IEquatable<TId>
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            stream.OnNext(Observable.Return(value));
        }
    }
}
