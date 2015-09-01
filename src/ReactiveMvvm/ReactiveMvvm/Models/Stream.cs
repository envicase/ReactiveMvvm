using System;
using System.Collections.Generic;
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

        public Stream(TId id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            Id = id;

            _subject = new BehaviorSubject<TModel>(value: null);
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

        private IEqualityComparer<TModel> EqualityComparer =>
            StreamStore<TModel, TId>.EqualityComparer ??
            EqualityComparer<TModel>.Default;

        private ICoalescer<TModel> Coalescer =>
            StreamStore<TModel, TId>.Coalescer ?? Coalescer<TModel>.Default;

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

            model = CoalesceWithLast(model);

            if (EqualityComparer.Equals(model, _subject.Value))
            {
                return;
            }

            _subject.OnNext(model);
        }

        private InvalidOperationException InvalidCoalescingResultId =>
            new InvalidOperationException(
                $"The id of the coalescing result"
                + $" is not equal to ${nameof(Id)}({Id}).");

        private TModel CoalesceWithLast(TModel model)
        {
            if (_subject.Value == null)
            {
                return model;
            }

            var result = Coalescer.Coalesce(model, _subject.Value);
            if (result.Id.Equals(Id) == false)
            {
                throw InvalidCoalescingResultId;
            }
            return result;
        }

        public void Dispose()
        {
            _subject.Dispose();
        }
    }
}
