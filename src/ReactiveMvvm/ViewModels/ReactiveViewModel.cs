using System;
using System.ComponentModel;

namespace ReactiveMvvm.ViewModels
{
    // TODO: ReactiveViewModel 클래스의 디자인이 성숙 단계에 이르면
    // XML 주석을 추가하고 아래 pragma 지시문을 제거합니다.
#pragma warning disable 1591

    public static class ReactiveViewModel
    {
        public static PropertyChangedEventArgs ModelChangedEventArgs { get; } =
            new PropertyChangedEventArgs("Model");
    }

#pragma warning restore 1591

    // TODO: ReactiveViewModel<TModel, TId> 클래스에 XML 주석이 작성되면 pragam
    // 지시문을 삭제해주세요.
#pragma warning disable 1591

    public class ReactiveViewModel<TModel, TId> : ObservableObject, IDisposable
        where TModel : class, IModel<TId>
        where TId : IEquatable<TId>
    {
        private static T NullArgumentGuard<T>(T value, string paramName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }

            return value;
        }

        private readonly IConnection<TModel, TId> _connection;
        private TModel _model;

        public ReactiveViewModel(IConnection<TModel, TId> connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            _connection = connection;
            _connection.Subscribe(model => Model = model);
        }

        public ReactiveViewModel(TId modelId)
            : this(Stream<TModel, TId>.Connect(
                NullArgumentGuard(modelId, nameof(modelId))))
        {
        }

        public ReactiveViewModel(TModel model)
            : this(Stream<TModel, TId>.Connect(
                NullArgumentGuard(model, nameof(model)).Id))
        {
            _connection.Emit(model);
        }

        public TId ModelId => _connection.ModelId;

        protected IConnection<TModel, TId> StreamConnection => _connection;

        public TModel Model
        {
            get
            {
                return _model;
            }
            private set
            {
                if (Equals(_model, value))
                {
                    return;
                }
                _model = value;
                OnPropertyChanged(ReactiveViewModel.ModelChangedEventArgs);
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) => _connection.Dispose();
    }

#pragma warning restore 1591
}
