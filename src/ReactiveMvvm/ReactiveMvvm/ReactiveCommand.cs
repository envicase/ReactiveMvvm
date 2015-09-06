using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ReactiveMvvm
{
    public static class ReactiveCommand
    {
        private static IObservable<Func<object, bool>> CanAlwaysExecute =>
            Observable.Return<Func<object, bool>>(_ => true);

        public static ReactiveCommand<Unit> Create(Action<object> execute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            return new ReactiveCommand<Unit>(
                CanAlwaysExecute,
                p =>
                {
                    execute.Invoke(p);
                    return Task.FromResult(Unit.Default);
                });
        }

        public static ReactiveCommand<Unit> Create(Func<object, Task> execute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            return new ReactiveCommand<Unit>(
                CanAlwaysExecute,
                async p =>
                {
                    await execute.Invoke(p);
                    return Unit.Default;
                });
        }

        public static ReactiveCommand<T> Create<T>(
            Func<object, Task<T>> execute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            return new ReactiveCommand<T>(CanAlwaysExecute, execute);
        }

        public static ReactiveCommand<T> Create<T>(
            IObservable<bool> canExecuteSource,
            Func<object, Task<T>> execute)
        {
            if (canExecuteSource == null)
            {
                throw new ArgumentNullException(nameof(canExecuteSource));
            }
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            return new ReactiveCommand<T>(
                canExecuteSource.Select(e => (Func<object, bool>)(_ => e)),
                execute);
        }

        public static ReactiveCommand<T> Create<T>(
            IObservable<Func<object, bool>> canExecuteSource,
            Func<object, Task<T>> execute)
        {
            if (canExecuteSource == null)
            {
                throw new ArgumentNullException(nameof(canExecuteSource));
            }
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            return new ReactiveCommand<T>(canExecuteSource, execute);
        }
    };

    public class ReactiveCommand<T> : ICommand, IObservable<T>
    {
        private Func<object, bool> _canExecute;
        private readonly Func<object, Task<T>> _execute;
        private readonly Subject<T> _spout;

        public ReactiveCommand(
            IObservable<Func<object, bool>> canExecuteSource,
            Func<object, Task<T>> execute)
        {
            if (canExecuteSource == null)
            {
                throw new ArgumentNullException(nameof(canExecuteSource));
            }
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            _execute = execute;
            _spout = new Subject<T>();

            canExecuteSource.Subscribe(value =>
            {
                _canExecute = value;
                RaiseCanExecuteChanged();
            });
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public bool CanExecute(object parameter) =>
            _canExecute?.Invoke(parameter) ?? true;

        public async void Execute(object parameter)
        {
            await ExecuteAsync(parameter);
        }

        public async Task ExecuteAsync(object parameter)
        {
            try
            {
                if (CanExecute(parameter))
                {
                    _spout.OnNext(await _execute.Invoke(parameter));
                }
            }
            catch (Exception error)
            {
                _spout.OnError(error);
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            var sub = _spout.Subscribe(
                observer.OnNext, observer.OnError, observer.OnCompleted);
            return Disposable.Create(sub.Dispose);
        }
    }
}
