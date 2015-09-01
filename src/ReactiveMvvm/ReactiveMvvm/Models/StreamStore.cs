using System;
using System.Collections.Generic;

namespace ReactiveMvvm.Models
{
    public static class StreamStore<TModel, TId>
        where TModel : Model<TId>
        where TId : IEquatable<TId>
    {
        private static readonly Dictionary<TId, Stream<TModel, TId>> _storage;

        static StreamStore()
        {
            _storage = new Dictionary<TId, Stream<TModel, TId>>();
        }

        public static IEqualityComparer<TModel> EqualityComparer { get; set; }

        public static IObservable<TModel> GetStream(TId id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            lock (_storage)
            {
                return GetStreamUnsafe(id);
            }
        }

        private static Stream<TModel, TId> GetStreamUnsafe(TId id)
        {
            Stream<TModel, TId> stream;
            if (_storage.TryGetValue(id, out stream))
            {
                return stream;
            }
            stream = new Stream<TModel, TId>(id);
            _storage[id] = stream;
            return stream;
        }

        public static void Push(TModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            lock (_storage)
            {
                GetStreamUnsafe(model.Id).Push(model);
            }
        }
    }
}
