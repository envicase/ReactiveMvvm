using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    public static class Stream<TModel, TId>
        where TModel : class, IModel<TId>
        where TId : IEquatable<TId>
    {
        private sealed class Instance
        {
            private readonly TId _modelId;
            private readonly Subject<IObservable<TModel>> _spout;
            private readonly BehaviorSubject<TModel> _subject;

            public Instance(TId modelId)
            {
                _modelId = modelId;
                _spout = new Subject<IObservable<TModel>>();
                _subject = new BehaviorSubject<TModel>(value: null);

                _spout.Switch().Subscribe(OnNext);
            }

            public TId ModelId => _modelId;

            public IObserver<IObservable<TModel>> Spout => _spout;

            public IObservable<TModel> Observable => _subject;

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
                new InvalidOperationException($"The id of the coalescing result is not equal to ({_modelId}).");

            public IDisposable Subscribe(IObserver<TModel> observer) =>
                new Subscription(this, observer);

            /// <summary>
            /// 스트리림 구독 수명을 관리합니다.
            /// </summary>
            /// <remarks>
            /// <see cref="Subscribe(IObserver{TModel})"/> 메서드가
            /// <see cref="System.Reactive.Disposables.Disposable"/> 클래스 대신
            /// 이 클래스를 사용하는 이유는 힙 메모리를 사용하는 대리자 개체를
            /// 추가적으로 생성하지 않기 위해서입니다.
            /// </remarks>
            private class Subscription : IDisposable
            {
                private readonly Instance _stream;
                private IDisposable _inner;

                public Subscription(Instance stream, IObserver<TModel> observer)
                {
                    _stream = stream;
                    _inner = WeakSubscription.Create(stream._subject, observer);
                }

                public void Dispose()
                {
                    IDisposable subscription =
                        Interlocked.Exchange(ref _inner, null);
                    if (subscription == null)
                    {
                        return;
                    }
                    lock (_syncRoot)
                    {
                        subscription.Dispose();
                        if (false == _stream._subject.HasObservers)
                        {
                            RemoveUnsafe(_stream._modelId);
                        }
                    }
                }
            }

            public void Dispose()
            {
                _spout.Dispose();
                _subject.Dispose();
            }
        }

        private sealed class Connection : IConnection<TModel, TId>
        {
            private readonly Instance _stream;
            private readonly BehaviorSubject<TModel> _subject;
            private readonly IObservable<TModel> _observable;
            private readonly IDisposable _subscription;

            public Connection(Instance stream)
            {
                _stream = stream;
                _subject = new BehaviorSubject<TModel>(value: null);
                _observable = from m in _subject
                              where m != null
                              select m;
                _subscription = _stream.Subscribe(_subject);
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

                _stream.Spout.OnNext(source);
            }

            public IDisposable Subscribe(IObserver<TModel> observer)
            {
                if (observer == null)
                {
                    throw new ArgumentNullException(nameof(observer));
                }

                return _observable.Subscribe(observer);
            }

            public void Dispose()
            {
                _subscription.Dispose();
                GC.SuppressFinalize(this);
            }
        }

        private static readonly object _syncRoot = new object();

        private static readonly Dictionary<TId, Instance> _store =
            new Dictionary<TId, Instance>();

        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Class wide equality comparer should be provided.")]
        public static IEqualityComparer<TModel> EqualityComparer { get; set; }

        private static IEqualityComparer<TModel> EqualityComparerSafe =>
            EqualityComparer ?? EqualityComparer<TModel>.Default;

        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Class wide coalescer should be provided.")]
        public static ICoalescer<TModel> Coalescer { get; set; }

        private static ICoalescer<TModel> CoalescerSafe =>
            Coalescer ?? Coalescer<TModel>.Default;

        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Class wide check function should be provided.")]
        public static bool ExistsFor(TId modelId)
        {
            lock (_syncRoot)
            {
                return _store.ContainsKey(modelId);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Class wide reset function should be provided.")]
        public static void Clear()
        {
            lock (_syncRoot)
            {
                foreach (var stream in _store.Values)
                {
                    stream.Dispose();
                }
                _store.Clear();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Class wide connect function should be provided.")]
        public static IConnection<TModel, TId> Connect(TId modelId)
        {
            lock (_syncRoot)
            {
                Instance stream;
                if (false == _store.TryGetValue(modelId, out stream))
                {
                    _store.Add(modelId, stream = new Instance(modelId));
                }
                return new Connection(stream);
            }
        }

        private static void RemoveUnsafe(TId modelId)
        {
            Instance stream;
            if (false == _store.TryGetValue(modelId, out stream))
            {
                return;
            }
            stream.Dispose();
            _store.Remove(modelId);
        }
    }
}
