using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ReactiveMvvm.Models
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.Design",
        "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable",
        Justification = "Dispose() should not be exposed publicly.")]
    internal class Stream<TModel, TId> : IObservable<TModel>
        where TModel : Model<TId>
        where TId : IEquatable<TId>
    {
        private readonly BehaviorSubject<TModel> _subject;
        private readonly IObservable<TModel> _observable;

        public Stream(TId id, TModel value = null)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            Id = id;

            _subject = new BehaviorSubject<TModel>(value);
            _observable = from m in _subject
                          select m;
        }

        public TId Id { get; }

        public IDisposable Subscribe(IObserver<TModel> observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            var sub = _observable.Subscribe(observer);
            return Disposable.Create(() => sub.Dispose());
        }

        public void Push(TModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            if (model.Id.Equals(Id) == false)
            {
                var message =
                    $"{nameof(model)}.{nameof(model.Id)}({model.Id})"
                    + $" is not equal to ${nameof(Id)}({Id}).";
                throw new ArgumentException(message, nameof(model));
            }

            _subject.OnNext(model);
        }

        public void Dispose()
        {
            _subject.Dispose();
        }
    }
}
