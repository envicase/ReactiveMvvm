using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ReactiveMvvm
{
    public class Stream<TModel, TId> : ISubject<IObservable<TModel>, TModel>
        where TModel : Model<TId>
        where TId : IEquatable<TId>
    {
        private static readonly object _syncRoot;

        private static readonly Dictionary
            <TId, WeakReference<Stream<TModel, TId>>> _store;

        static Stream()
        {
            _syncRoot = new object();
            _store = new Dictionary<TId, WeakReference<Stream<TModel, TId>>>();
        }

        public static IEqualityComparer<TModel> EqualityComparer { get; set; }

        private static IEqualityComparer<TModel> EqualityComparerSafe =>
            EqualityComparer ?? EqualityComparer<TModel>.Default;

        public static ICoalescer<TModel> Coalescer { get; set; }

        private static ICoalescer<TModel> CoalescerSafe =>
            Coalescer ?? Coalescer<TModel>.Default;

        private static void Invoke(Action action)
        {
            lock (_syncRoot)
            {
                action.Invoke();
            }
        }

        private static T Invoke<T>(Func<T> func)
        {
            lock (_syncRoot)
            {
                return func.Invoke();
            }
        }

        public static Stream<TModel, TId> Get(TId id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return Invoke(() => GetUnsafe(id));
        }

        private static Stream<TModel, TId> GetUnsafe(TId id)
        {
            WeakReference<Stream<TModel, TId>> reference;
            if (false == _store.TryGetValue(id, out reference))
            {
                _store[id] = reference =
                    new WeakReference<Stream<TModel, TId>>(
                        new Stream<TModel, TId>(id));
            }
            Stream<TModel, TId> stream;
            reference.TryGetTarget(out stream);
            return stream;
        }

        private static void Remove(TId id) => Invoke(() => RemoveUnsafe(id));

        private static void RemoveUnsafe(TId id) => _store.Remove(id);

        public static void Clear() => Invoke(ClearUnsafe);

        private static void ClearUnsafe()
        {
            foreach (var reference in _store.Values)
            {
                Stream<TModel, TId> stream;
                if (reference.TryGetTarget(out stream))
                {
                    stream._innerSubject.Dispose();
                }
            }
            _store.Clear();
        }

        public TId Id { get; }

        private readonly BehaviorSubject<TModel> _innerSubject;
        private readonly IObservable<TModel> _observable;
        private readonly Subject<IObservable<TModel>> _spout;

        private Stream(TId id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            Id = id;

            _innerSubject = new BehaviorSubject<TModel>(value: null);
            _observable = from m in _innerSubject select m;
            _spout = new Subject<IObservable<TModel>>();
            _spout.Switch().Subscribe(OnNext);
        }

        ~Stream()
        {
            Remove(Id);
        }

        private InvalidOperationException InvalidCoalescingResultId =>
            new InvalidOperationException(
                $"The id of the coalescing result"
                + $" is not equal to ({Id}).");

        private TModel CoalesceWithLast(TModel model)
        {
            if (_innerSubject.Value == null)
            {
                return model;
            }

            var result = CoalescerSafe.Coalesce(model, _innerSubject.Value);
            if (result.Id.Equals(Id) == false)
            {
                throw InvalidCoalescingResultId;
            }
            return result;
        }

        public IDisposable Subscribe(IObserver<TModel> observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            return _observable.Subscribe(observer);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        void IObserver<IObservable<TModel>>.OnCompleted()
        {
            throw new NotSupportedException("This operation is not supported.");
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        void IObserver<IObservable<TModel>>.OnError(Exception error)
        {
            throw new NotSupportedException("This operation is not supported.");
        }

        public void OnNext(IObservable<TModel> observable)
        {
            if (observable == null)
            {
                throw new ArgumentNullException(nameof(observable));
            }

            _spout.OnNext(observable);
        }

        private void OnNext(TModel value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (value.Id.Equals(Id) == false)
            {
                var message =
                    $"{nameof(value)}.{nameof(value.Id)}({value.Id})"
                    + $" is not equal to ({Id}).";
                throw new ArgumentException(message, nameof(value));
            }

            var comparer = EqualityComparerSafe;

            if (comparer.Equals(value, _innerSubject.Value))
            {
                return;
            }

            var model = CoalesceWithLast(value);

            if (comparer.Equals(model, _innerSubject.Value))
            {
                return;
            }

            _innerSubject.OnNext(model);
        }
    }
}
