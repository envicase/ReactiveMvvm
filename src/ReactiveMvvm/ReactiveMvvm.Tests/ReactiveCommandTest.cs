using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using FluentAssertions;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace ReactiveMvvm.Tests
{
    public class ReactiveCommandTest
    {
        [Fact]
        public void IsICommand()
        {
            var sut = ReactiveCommand.Create();
            sut.Should().BeAssignableTo<ICommand>();
        }

        [Fact]
        public void RaiseCanExecuteChangedRaisesEvent()
        {
            var sut = ReactiveCommand.Create();
            sut.MonitorEvents();

            sut.RaiseCanExecuteChanged();

            sut.ShouldRaise(nameof(sut.CanExecuteChanged))
                .WithSender(sut).WithArgs<EventArgs>(a => a == EventArgs.Empty);
        }

        [Theory, AutoData]
        public void InitializesWithNoParameter(object parameter)
        {
            var command = ReactiveCommand.Create();
            command.Should().NotBeNull();
            command.CanExecute(parameter).Should().BeTrue();
        }

        [Theory, AutoData]
        public void InitializesWithResultlessAsyncExecuteImpl(
            object parameter)
        {
            var functor = Mock.Of<IFunctor>();

            var command = ReactiveCommand.Create(
                p => functor.Func<object, Task>(p));
            command?.Execute(parameter);

            command.Should().NotBeNull();
            command.CanExecute(parameter).Should().BeTrue();
            Mock.Get(functor).Verify(f =>
                f.Func<object, Task>(parameter), Times.Once());
        }

        [Theory, AutoData]
        public void InitializesWithCanExecuteValueSource(
            Subject<bool> source, object parameter)
        {
            var functor = Mock.Of<IFunctor>();

            var command = ReactiveCommand.Create(
                source, functor.Func<object, Task<Unit>>);
            command?.MonitorEvents();
            source.OnNext(false);

            command.Should().NotBeNull();
            command.ShouldRaise(nameof(command.CanExecuteChanged));
            command.CanExecute(parameter).Should().BeFalse();
        }

        [Theory, AutoData]
        public void InitializesWithSyncExecuteAction(object parameter)
        {
            var functor = Mock.Of<IFunctor>();

            var command = ReactiveCommand.Create(p => functor.Action(p));
            command?.Execute(parameter);

            command.Should().NotBeNull();
            command.CanExecute(parameter).Should().BeTrue();
            Mock.Get(functor).Verify(f => f.Action(parameter), Times.Once());
        }

        [Theory, AutoData]
        public void InitializesWithSyncExecuteFunc(object parameter)
        {
            var functor = Mock.Of<IFunctor>();

            var command = ReactiveCommand.Create(
                p => functor.Func<object, object>(p));
            command?.Execute(parameter);

            command.Should().NotBeNull();
            command.CanExecute(parameter).Should().BeTrue();
            Mock.Get(functor).Verify(f =>
                f.Func<object, object>(parameter), Times.Once());
        }

        [Theory, AutoData]
        public void InitializesWithCanExecuteFuncAndExecuteAction(
            object parameter)
        {
            var functor = Mock.Of<IFunctor>(f =>
                f.Func<object, bool>(parameter) == true);

            var command = ReactiveCommand.Create(
                functor.Func<object, bool>, functor.Action);
            command?.Execute(parameter);

            command.Should().NotBeNull();
            Mock.Get(functor).Verify(f =>
                f.Func<object, bool>(parameter), Times.Once());
            Mock.Get(functor).Verify(f => f.Action(parameter), Times.Once());
        }

        [Theory, AutoData]
        public void RaisesCanExecuteChangedWithNextCanExecuteImplementation(
            Subject<Func<object, bool>> source, object parameter)
        {
            var sut = new ReactiveCommand<Unit>(
                source, _ => Task.FromResult(Unit.Default));
            sut.MonitorEvents();

            source.OnNext(p => p == parameter);

            sut.ShouldRaise(nameof(sut.CanExecuteChanged))
                .WithSender(sut).WithArgs<EventArgs>(a => a == EventArgs.Empty);
            sut.CanExecute(parameter).Should().BeTrue();
            sut.CanExecute(new object()).Should().BeFalse();
        }

        [Theory, AutoData]
        public async Task ExecuteInvokesDelegate(object parameter)
        {
            var functor = Mock.Of<IFunctor>(f =>
                f.Func<object, Task<Unit>>(parameter) ==
                    Task.FromResult(Unit.Default));
            var sut = ReactiveCommand.Create(functor.Func<object, Task<Unit>>);

            await sut.ExecuteAsync(parameter);

            Mock.Get(functor).Verify(f =>
                f.Func<object, Task<Unit>>(parameter), Times.Once());
        }

        [Theory, AutoData]
        public async Task ExecuteSendsUnit(object parameter)
        {
            var functor = Mock.Of<IFunctor>(f =>
                f.Func<object, Task<Unit>>(parameter) ==
                    Task.FromResult(Unit.Default));
            var sut = ReactiveCommand.Create(functor.Func<object, Task<Unit>>);
            object actual = null;
            sut.Subscribe(u => actual = u);

            await sut.ExecuteAsync(parameter);

            actual.Should().NotBeNull();
        }

        [Theory, AutoData]
        public async Task ExecuteSendsThrownException(
            object parameter, InvalidOperationException error)
        {
            var functor = Mock.Of<IFunctor>();
            Mock.Get(functor)
                .Setup(f => f.Func<object, Task<Unit>>(parameter))
                .Throws(error);
            var sut = ReactiveCommand.Create(functor.Func<object, Task<Unit>>);
            var exceptions = new List<Exception>();
            sut.Subscribe(_ => { }, onError: e => exceptions.Add(e));
            sut.Subscribe(_ => { }, onError: e => exceptions.Add(e));

            await sut.ExecuteAsync(parameter);

            exceptions.Should().Equal(error, error);
        }

        private class DelegatingScheduler : IScheduler
        {
            private IScheduler _scheduler;

            public DelegatingScheduler(IScheduler scheduler)
            {
                _scheduler = scheduler;
            }

            public DateTimeOffset Now => _scheduler.Now;

            public IDisposable Schedule<TState>(
                TState state, Func<IScheduler, TState, IDisposable> action) =>
                _scheduler.Schedule<object>(
                    state, (arg1, arg2) => action.Invoke(arg1, (TState)arg2));

            public IDisposable Schedule<TState>(
                TState state,
                DateTimeOffset dueTime,
                Func<IScheduler, TState, IDisposable> action)
            {
                throw new NotSupportedException();
            }

            public IDisposable Schedule<TState>(
                TState state,
                TimeSpan dueTime,
                Func<IScheduler, TState, IDisposable> action)
            {
                throw new NotSupportedException();
            }
        }

        [Fact]
        public void CanExecuteSourceIsObservedOnProvidedScheduler()
        {
            var scheduler = Mock.Of<IScheduler>();
            var canExecuteSource = new Subject<bool>();
            var sut = ReactiveCommand.Create(
                canExecuteSource, _ => Task.FromResult(Unit.Default));
            sut.Scheduler = new DelegatingScheduler(scheduler);

            canExecuteSource.OnNext(false);

            Mock.Get(scheduler).Verify(s =>
                s.Schedule(
                    It.IsNotNull<object>(),
                    It.IsNotNull<Func<IScheduler, object, IDisposable>>()),
                Times.Once());
        }

        [Theory, AutoData]
        public void ExecutionIsObservedOnProvidedScheduler(object parameter)
        {
            var scheduler = Mock.Of<IScheduler>();
            var sut = ReactiveCommand.Create(_ => _);
            sut.Scheduler = new DelegatingScheduler(scheduler);
            sut.Subscribe();

            sut.Execute(parameter);

            Mock.Get(scheduler).Verify(s =>
                s.Schedule(
                    It.IsNotNull<object>(),
                    It.IsNotNull<Func<IScheduler, object, IDisposable>>()),
                Times.Once());
        }
    }
}
