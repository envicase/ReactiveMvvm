using System;
using System.ComponentModel;

namespace ReactiveMvvm
{
#pragma warning disable 1591
    public sealed class StreamConnection<TModel, TId> :
        IObserver<TModel>, IDisposable
        where TModel : class, IModel<TId>
        where TId : IEquatable<TId>
    {
        // StreamConnection<TModel, TId>는 스트림에 대한 참조를 필드에 유지하고
        // WeakSubscription<T> 클래스를 사용하여 스트림을 구독합니다. 결과적으로
        // 스트림 연결은 스트림에 대한 강한 참조를, 스트림은 스트림 연결에 대한
        // 약한 참조를 가지게 됩니다.
        //
        // +------------------+            +------------+
        // | StreamConnection |            |   Stream   |
        // |                  |----------->|            |
        // |                  |  <strong>  |            |
        // |                  |            |            |
        // |                  |<- - - - - -|            |
        // |                  |   <weak>   |            |
        // +------------------+            +------------+

        private readonly Stream<TModel, TId> _stream;
        private readonly Action<TModel> _onNext;
        private readonly IDisposable _subscription;

        public StreamConnection(TId id, Action<TModel> onNext)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            if (onNext == null)
            {
                throw new ArgumentNullException(nameof(onNext));
            }

            _stream = Stream<TModel, TId>.Get(id);
            _onNext = onNext;
            _subscription = WeakSubscription.Create(_stream, this);
        }

        ~StreamConnection()
        {
            Dispose();
        }

        public Stream<TModel, TId> Stream => _stream;

        void IObserver<TModel>.OnCompleted()
        {
        }

        void IObserver<TModel>.OnError(Exception error)
        {
        }

        void IObserver<TModel>.OnNext(TModel value) => _onNext.Invoke(value);

        public void Dispose()
        {
            _subscription.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
