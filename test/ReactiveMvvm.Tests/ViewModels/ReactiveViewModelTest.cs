using System;
using System.ComponentModel;
using FluentAssertions;
using Ploeh.AutoFixture.Xunit2;
using ReactiveMvvm.ViewModels;
using Xunit;

namespace ReactiveMvvm.Tests.ViewModels
{
    using static Stream<User, string>;

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
            var sut = new ReactiveViewModel<User, string>(user.Id);
            sut.MonitorEvents();

            Connect(user.Id).Emit(user);

            sut.Model.Should().BeSameAs(user);
            sut.ShouldRaisePropertyChangeFor(x => x.Model);
        }

        [Theory, AutoData]
        public void ModelSetterRaisesEventWithModelChangedEventArgs(User user)
        {
            var sut = new ReactiveViewModel<User, string>(user.Id);
            sut.MonitorEvents();

            Connect(user.Id).Emit(user);

            sut.ShouldRaisePropertyChangeFor(x => x.Model)
                .WithArgs<PropertyChangedEventArgs>(args => ReferenceEquals(
                    args, ReactiveViewModel.ModelChangedEventArgs));
        }
    }
}
