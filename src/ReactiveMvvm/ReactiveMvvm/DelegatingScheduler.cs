using System;
using System.Reactive.Concurrency;

namespace ReactiveMvvm
{
    internal class DelegatingScheduler : IScheduler
    {
        private readonly Func<IScheduler> _provider;

        public DelegatingScheduler(Func<IScheduler> provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            _provider = provider;
        }

        private IScheduler Implementor => _provider.Invoke();

        public DateTimeOffset Now => Implementor.Now;

        public IDisposable Schedule<TState>(
            TState state, Func<IScheduler, TState, IDisposable> action) =>
            Implementor.Schedule(state, action);

        public IDisposable Schedule<TState>(
            TState state,
            DateTimeOffset dueTime,
            Func<IScheduler, TState, IDisposable> action) =>
            Implementor.Schedule(state, dueTime, action);

        public IDisposable Schedule<TState>(
            TState state,
            TimeSpan dueTime,
            Func<IScheduler, TState, IDisposable> action) =>
            Implementor.Schedule(state, dueTime, action);
    }
}
