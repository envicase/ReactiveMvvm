using System;

namespace ReactiveMvvm.ViewModels
{
    public class ReactiveViewModel<TModel, TId> : ObservableObject
        where TModel : Model<TId>
        where TId : IEquatable<TId>
    {
        private readonly StreamConnection<TModel, TId> _connection;
        private TModel _model;

        public ReactiveViewModel(TId id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            _connection = new StreamConnection<TModel, TId>(id, m => Model = m);
        }

        public TId Id => _connection.Stream.Id;

        protected Stream<TModel, TId> Stream => _connection.Stream;

        public TModel Model
        {
            get { return _model; }
            private set { SetValue(ref _model, value); }
        }
    }
}
