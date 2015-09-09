using System;

namespace ReactiveMvvm
{
    // TODO: WeakSubscription 클래스에 XML 주석이 작성되면 pragam
    // 지시문을 삭제해주세요.
#pragma warning disable 1591
    public static class WeakSubscription
    {
        public static WeakSubscription<T> Create<T>(
            IObservable<T> observable, IObserver<T> observer)
        {
            if (observable == null)
            {
                throw new ArgumentNullException(nameof(observable));
            }
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            return new WeakSubscription<T>(observable, observer);
        }
    }
#pragma warning restore 1591

    // TODO: WeakSubscription<T> 클래스에 XML 주석이 작성되면 pragam
    // 지시문을 삭제해주세요.
#pragma warning disable 1591
    public sealed class WeakSubscription<T> : IDisposable
    {
        private readonly WeakReference<IObserver<T>> _reference;
        private readonly IDisposable _subscription;

        public WeakSubscription(
            IObservable<T> observable, IObserver<T> observer)
        {
            if (observable == null)
            {
                throw new ArgumentNullException(nameof(observable));
            }
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            _reference = new WeakReference<IObserver<T>>(observer);
            _subscription = observable.Subscribe(OnNext, OnError, OnCompleted);
        }

        private void OnNext(T value)
        {
            IObserver<T> observer;
            if (_reference.TryGetTarget(out observer))
            {
                observer.OnNext(value);
            }
            else
            {
                _subscription.Dispose();
            }
        }

        private void OnError(Exception error)
        {
            IObserver<T> observer;
            if (_reference.TryGetTarget(out observer))
            {
                observer.OnError(error);
            }
            else
            {
                _subscription.Dispose();
            }
        }

        private void OnCompleted()
        {
            IObserver<T> observer;
            if (_reference.TryGetTarget(out observer))
            {
                observer.OnCompleted();
            }
            else
            {
                _subscription.Dispose();
            }
        }

        public void Dispose() => _subscription.Dispose();
    }
}
