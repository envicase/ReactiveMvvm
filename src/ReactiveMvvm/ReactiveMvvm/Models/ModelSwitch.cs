using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ReactiveMvvm.Models
{
    public class ModelSwitch<TModel, TId> : IObserver<IObservable<TModel>>
        where TModel : Model<TId>
        where TId : IEquatable<TId>
    {
        private readonly Stream<TModel, TId> _stream;
        private readonly Subject<IObservable<TModel>> _spout;

        public ModelSwitch(TId id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            _stream = Stream<TModel, TId>.Get(id);
            _spout = new Subject<IObservable<TModel>>();
            _spout.Switch().Subscribe(value => _stream.OnNext(value));
        }

        public TId Id => _stream.Id;

        [EditorBrowsable(EditorBrowsableState.Never)]
        void IObserver<IObservable<TModel>>.OnCompleted()
        {
            throw new NotSupportedException();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        void IObserver<IObservable<TModel>>.OnError(Exception error)
        {
            throw new NotSupportedException();
        }

        public void OnNext(IObservable<TModel> observable)
        {
            if (observable == null)
            {
                throw new ArgumentNullException(nameof(observable));
            }

            _spout.OnNext(observable);
        }
    }
}
