using System;
using System.Reactive.Concurrency;

namespace ReactiveMvvm
{
    internal class DelegatingScheduler : IScheduler
    {
        private readonly Func<IScheduler> _factory;

        public DelegatingScheduler(Func<IScheduler> factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            _factory = factory;
        }

        private IScheduler Implementor => _factory.Invoke();

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
