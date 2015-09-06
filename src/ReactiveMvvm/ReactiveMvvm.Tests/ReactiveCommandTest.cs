using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Input;
using FluentAssertions;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace ReactiveMvvm.Tests
{
    public class ReactiveCommandTest
    {
        [Fact]
        public void IsICommand()
        {
            var sut = ReactiveCommand.Create(
                _ => Task.FromResult(Unit.Default));
            sut.Should().BeAssignableTo<ICommand>();
        }

        [Fact]
        public void RaiseCanExecuteChangedRaisesEvent()
        {
            var sut = ReactiveCommand.Create(
                _ => Task.FromResult(Unit.Default));
            sut.MonitorEvents();

            sut.RaiseCanExecuteChanged();

            sut.ShouldRaise(nameof(sut.CanExecuteChanged))
                .WithSender(sut).WithArgs<EventArgs>(a => a == EventArgs.Empty);
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
        public void InitializesWithResultlessSyncExecuteImpl(
            object parameter)
        {
            var functor = Mock.Of<IFunctor>();

            var command = ReactiveCommand.Create(p => functor.Action(p));
            command?.Execute(parameter);

            command.Should().NotBeNull();
            command.CanExecute(parameter).Should().BeTrue();
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
    }
}
