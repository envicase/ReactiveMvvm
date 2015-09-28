using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace ReactiveMvvm
{
    // TODO: Stream<TModel, TId> 클래스에 XML 주석이 작성되면 pragam 지시문을
    // 삭제해주세요.
#pragma warning disable 1591

    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "This class provides not streams of bytes but streams of model instances.")]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Streams should not be disposed outside the class.")]
    public sealed class Stream<TModel, TId>
        where TModel : class, IModel<TId>
        where TId : IEquatable<TId>
    {
        private static readonly object _syncRoot = new object();

        private static readonly Dictionary<TId, Stream<TModel, TId>> _store =
            new Dictionary<TId, Stream<TModel, TId>>();

        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Class wide equality comparer should be provided.")]
        public static IEqualityComparer<TModel> EqualityComparer { get; set; }

        private static IEqualityComparer<TModel> EqualityComparerSafe =>
            EqualityComparer ?? EqualityComparer<TModel>.Default;

        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Class wide coalescer should be provided.")]
        public static ICoalescer<TModel> Coalescer { get; set; }

        private static ICoalescer<TModel> CoalescerSafe =>
            Coalescer ?? Coalescer<TModel>.Default;

        private static void InvokeWithLock(Action action)
        {
            lock (_syncRoot)
            {
                action.Invoke();
            }
        }

        private static T InvokeWithLock<T>(Func<T> func)
        {
            lock (_syncRoot)
            {
                return func.Invoke();
            }
        }

        private static void RemoveUnsafe(TId modelId) => _store.Remove(modelId);

        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Class wide reset function should be provided.")]
        public static void Clear() => InvokeWithLock(ClearUnsafe);

        private static void ClearUnsafe()
        {
            foreach (var stream in _store.Values)
            {
                stream.Dispose();
            }
            _store.Clear();
        }

        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Class wide connect function should be provided.")]
        public static IConnection<TModel, TId> Connect(TId modelId) =>
            InvokeWithLock(() => ConnectUnsafe(modelId));

        private static IConnection<TModel, TId> ConnectUnsafe(TId modelId)
        {
            Stream<TModel, TId> stream;
            if (false == _store.TryGetValue(modelId, out stream))
            {
                _store.Add(modelId, stream = new Stream<TModel, TId>(modelId));
            }
            return stream.Connect();
        }

        private readonly TId _modelId;
        private readonly BehaviorSubject<TModel> _subject;
        private readonly Subject<IObservable<TModel>> _spout;
        private int _connectionCount;

        private Stream(TId modelId)
        {
            if (modelId == null)
            {
                throw new ArgumentNullException(nameof(modelId));
            }

            _modelId = modelId;
            _subject = new BehaviorSubject<TModel>(value: null);
            _spout = new Subject<IObservable<TModel>>();
            _connectionCount = 0;

            _spout.Switch().Subscribe(OnNext);
        }

        internal TId ModelId => _modelId;

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object[])", Justification = "No argument to be formatted.")]
        private void OnNext(TModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            if (model.Id == null)
            {
                var message =
                    $"{nameof(model)}.{nameof(model.Id)} cannot be null.";
                throw new ArgumentException(message, nameof(model));
            }
            if (model.Id.Equals(_modelId) == false)
            {
                var message =
                    $"{nameof(model)}.{nameof(model.Id)}({model.Id})"
                    + $" is not equal to ({_modelId}).";
                throw new ArgumentException(message, nameof(model));
            }

            var comparer = EqualityComparerSafe;

            if (comparer.Equals(model, _subject.Value))
            {
                return;
            }

            var next = CoalesceWithLast(model);

            if (comparer.Equals(next, _subject.Value))
            {
                return;
            }

            _subject.OnNext(next);
        }

        private TModel CoalesceWithLast(TModel model)
        {
            if (_subject.Value == null)
            {
                return model;
            }

            var result = CoalescerSafe.Coalesce(model, _subject.Value);
            if (result.Id.Equals(_modelId) == false)
            {
                throw InvalidCoalescingResultId;
            }
            return result;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object[])", Justification = "No argument to be formatted.")]
        private InvalidOperationException InvalidCoalescingResultId =>
            new InvalidOperationException(
                "The id of the coalescing result"
                + $" is not equal to ({_modelId}).");

        private IConnection<TModel, TId> Connect()
        {
            return new Connection(this);
        }

        private sealed class Connection : IConnection<TModel, TId>
        {
            private readonly Stream<TModel, TId> _stream;
            private int _shares;

            public Connection(Stream<TModel, TId> stream)
            {
                if (stream == null)
                {
                    throw new ArgumentNullException(nameof(stream));
                }

                _stream = stream;
                _shares = 1;
                Interlocked.Add(ref _stream._connectionCount, _shares);
            }

            ~Connection()
            {
                Dispose();
            }

            public TId ModelId => _stream.ModelId;

            public void Emit(IObservable<TModel> source)
            {
                if (source == null)
                {
                    throw new ArgumentNullException(nameof(source));
                }

                _stream._spout.OnNext(source);
            }

            public IDisposable Subscribe(IObserver<TModel> observer)
            {
                if (observer == null)
                {
                    throw new ArgumentNullException(nameof(observer));
                }

                return _stream._subject.Subscribe(observer);
            }

            public void Dispose()
            {
                InvokeWithLock(() =>
                {
                    if (Interlocked.Exchange(ref _shares, 0) > 0)
                    {
                        Interlocked.Decrement(ref _stream._connectionCount);
                        if (_stream._connectionCount == 0)
                        {
                            RemoveUnsafe(_stream.ModelId);
                        }
                    }
                });
                GC.SuppressFinalize(this);
            }
        }

        private void Dispose()
        {
            _spout.Dispose();
            _subject.Dispose();
        }
    }
}
