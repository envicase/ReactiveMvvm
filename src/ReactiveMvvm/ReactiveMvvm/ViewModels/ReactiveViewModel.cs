using System;

namespace ReactiveMvvm.ViewModels
{
#pragma warning disable 1591
    public class ReactiveViewModel<TModel, TId> : ObservableObject, IDisposable
        where TModel : class, IModel<TId>
        where TId : IEquatable<TId>
    {
        private static T NullArgumentGuard<T>(T value, string paramName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }

            return value;
        }

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

        public ReactiveViewModel(TModel model)
            : this(NullArgumentGuard(model, nameof(model)).Id)
        {
            Stream.OnNext(model);
        }

        public TId Id => _connection.Stream.Id;

        protected Stream<TModel, TId> Stream => _connection.Stream;

        public TModel Model
        {
            get { return _model; }
            private set { SetValue(ref _model, value); }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) => _connection.Dispose();
    }
}
