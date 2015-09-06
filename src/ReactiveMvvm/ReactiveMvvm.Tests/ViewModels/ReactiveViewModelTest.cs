using System;
using System.Reactive.Linq;
using FluentAssertions;
using Ploeh.AutoFixture.Xunit2;
using ReactiveMvvm.ViewModels;
using Xunit;

namespace ReactiveMvvm.Tests.ViewModels
{
    [Collection("Using Stream<User, string>")]
    [ClearStreamAfterTest(typeof(User), typeof(string))]
    public class ReactiveViewModelTest
    {
        [Fact]
        public void ModelPropertyIsReadOnly()
        {
            var sut = typeof(ReactiveViewModel<,>).GetProperty("Model");

            sut.Should().NotBeNull();
            sut.SetMethod.Should().NotBeNull();
            sut.SetMethod.IsPrivate.Should().BeTrue();
        }

        [Theory, AutoData]
        public void SubscribesStream(User user)
        {
            var stream = Stream<User, string>.Get(user.Id);
            var sut = new ReactiveViewModel<User, string>(user.Id);
            sut.MonitorEvents();

            stream.OnNext(Observable.Return(user));

            sut.Model.Should().BeSameAs(user);
            sut.ShouldRaisePropertyChangeFor(x => x.Model);
        }
    }
}
