using System;

namespace ReactiveMvvm.ViewModels
{
    public class ReactiveViewModel<TModel, TId> : ObservableObject
        where TModel : Model<TId>
        where TId : IEquatable<TId>
    {
        private readonly WeakStreamObserver<TModel, TId> _observer;
        private TModel _model;

        public ReactiveViewModel(TId id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            Id = id;
            _observer = new WeakStreamObserver<TModel, TId>(id, m => Model = m);
        }

        public TId Id { get; }

        public TModel Model
        {
            get { return _model; }
            private set { SetValue(ref _model, value); }
        }
    }
}
