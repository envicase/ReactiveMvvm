using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ReactiveMvvm
{
    // TODO: ReactiveCommand 클래스에 XML 주석이 작성되면 pragam 지시문을
    // 삭제해주세요.
#pragma warning disable 1591

    public static class ReactiveCommand
    {
        private static IObservable<Func<object, bool>> CanAlwaysExecute =>
            Observable.Return<Func<object, bool>>(_ => true);

        public static ReactiveCommand<object> Create() =>
            new ReactiveCommand<object>(
                CanAlwaysExecute, p => Task.FromResult(p));

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

        public static ReactiveCommand<T> Create<T>(Func<object, T> execute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            return new ReactiveCommand<T>(
                CanAlwaysExecute, p => Task.FromResult(execute.Invoke(p)));
        }

        public static ReactiveCommand<Unit> Create(
            Func<object, bool> canExecute, Action<object> execute)
        {
            return new ReactiveCommand<Unit>(
                Observable.Return(canExecute),
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

#pragma warning restore 1591

    // TODO: ReactiveCommand<T> 클래스에 XML 주석이 작성되면 pragam 지시문을
    // 삭제해주세요.
#pragma warning disable 1591

    public class ReactiveCommand<T> : ICommand, IObservable<T>, IDisposable
    {
        private readonly IScheduler _scheduler;
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

            _scheduler = new DelegatingScheduler(() => SchedulerSafe);
            _execute = execute;
            _spout = new Subject<T>();

            canExecuteSource
                .ObserveOn(_scheduler)
                .Subscribe(OnNextCanExecuteSource);
        }

        private IScheduler SchedulerSafe =>
            Scheduler ?? ImmediateScheduler;

        private IScheduler ImmediateScheduler =>
            System.Reactive.Concurrency.Scheduler.Immediate;

        public IScheduler Scheduler { get; set; }

        private void OnNextCanExecuteSource(Func<object, bool> canExecute)
        {
            _canExecute = canExecute;
            RaiseCanExecuteChanged();
        }

        public event EventHandler CanExecuteChanged;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "'Raise' prefix is generally used to implement ICommand interface and to expose event fire function for PCLs.")]
        public void RaiseCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public bool CanExecute(object parameter) =>
            _canExecute?.Invoke(parameter) ?? true;

        public async void Execute(object parameter) =>
            await ExecuteAsync(parameter);

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

            return _spout.ObserveOn(_scheduler).Subscribe(observer);
        }

        protected virtual void Dispose(bool disposing) => _spout.Dispose();

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
