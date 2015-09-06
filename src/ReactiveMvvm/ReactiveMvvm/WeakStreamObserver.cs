using System;
using System.ComponentModel;

namespace ReactiveMvvm
{
    public class WeakStreamObserver<TModel, TId> :
        IObserver<TModel>, IDisposable
        where TModel : Model<TId>
        where TId : IEquatable<TId>
    {
        private readonly Action<TModel> _onNext;
        private readonly IDisposable _subscription;

        public WeakStreamObserver(TId id, Action<TModel> onNext)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            if (onNext == null)
            {
                throw new ArgumentNullException(nameof(onNext));
            }

            _onNext = onNext;

            var stream = Stream<TModel, TId>.Get(id);
            _subscription = WeakSubscription.Create(stream, this);
        }

        ~WeakStreamObserver()
        {
            Dispose();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        void IObserver<TModel>.OnCompleted()
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        void IObserver<TModel>.OnError(Exception error)
        {
        }

        void IObserver<TModel>.OnNext(TModel value) => _onNext.Invoke(value);

        public void Dispose() => _subscription.Dispose();
    }
}
