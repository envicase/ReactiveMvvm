using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using FluentAssertions;
using Ploeh.AutoFixture.Xunit2;
using ReactiveMvvm.Models;
using Xunit;

namespace ReactiveMvvm.Tests.Models
{
    [Collection("Using Stream<User, string>")]
    [ClearStreamAfterTest(typeof(User), typeof(string))]
    public class ModelSwitchTest
    {
        [Theory, AutoData]
        public void OnNextSendsModelToStreamWithObservable(User user)
        {
            var sut = new ModelSwitch<User, string>(user.Id);
            User actual = null;
            Stream<User, string>.Get(user.Id).Subscribe(u => actual = u);
            var observable = Task.FromResult(user).ToObservable();

            sut.OnNext(observable);

            actual.Should().BeSameAs(user);
        }

        [Theory, AutoData]
        public async Task OnNextSwitchesWithObservable(User user, string bio)
        {
            var sut = new ModelSwitch<User, string>(user.Id);
            User actual = null;
            Stream<User, string>.Get(user.Id).Subscribe(u => actual = u);
            var task = Task.Delay(10).ContinueWith(_ => user);

            sut.OnNext(task.ToObservable());
            sut.OnNext(Task.FromResult(
                new User(user.Id, user.UserName, bio)).ToObservable());
            await task;
            await Task.Delay(10);

            actual.Should().NotBeNull();
            actual.Bio.Should().Be(bio);
        }

        [Theory, AutoData]
        public void OnNextSendsModelToStreamWithTask(User user)
        {
            var sut = new ModelSwitch<User, string>(user.Id);
            User actual = null;
            Stream<User, string>.Get(user.Id).Subscribe(u => actual = u);
            var task = Task.FromResult(user);

            sut.OnNext(task);

            actual.Should().BeSameAs(user);
        }

        [Theory, AutoData]
        public async Task OnNextSwitchesWithTask(User user, string bio)
        {
            var sut = new ModelSwitch<User, string>(user.Id);
            User actual = null;
            Stream<User, string>.Get(user.Id).Subscribe(u => actual = u);
            var task = Task.Delay(10).ContinueWith(_ => user);

            sut.OnNext(task);
            sut.OnNext(Task.FromResult(new User(user.Id, user.UserName, bio)));
            await task;
            await Task.Delay(10);

            actual.Should().NotBeNull();
            actual.Bio.Should().Be(bio);
        }

        [Theory, AutoData]
        public void OnNextSendsModelToStreamWithValue(User user)
        {
            var sut = new ModelSwitch<User, string>(user.Id);
            User actual = null;
            Stream<User, string>.Get(user.Id).Subscribe(u => actual = u);

            sut.OnNext(user);

            actual.Should().BeSameAs(user);
        }

        [Theory, AutoData]
        public async Task OnNextSwitchesWithValue(User user, string bio)
        {
            var sut = new ModelSwitch<User, string>(user.Id);
            User actual = null;
            Stream<User, string>.Get(user.Id).Subscribe(u => actual = u);
            var task = Task.Delay(10).ContinueWith(_ => user);

            sut.OnNext(task);
            sut.OnNext(new User(user.Id, user.UserName, bio));
            await task;
            await Task.Delay(10);

            actual.Should().NotBeNull();
            actual.Bio.Should().Be(bio);
        }
    }
}
