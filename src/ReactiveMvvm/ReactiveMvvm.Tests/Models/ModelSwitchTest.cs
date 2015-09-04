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
            sut.OnNext(Observable.Return(
                new User(user.Id, user.UserName, bio)));
            await task;
            await Task.Delay(10);

            actual.Should().NotBeNull();
            actual.Bio.Should().Be(bio);
        }
    }
}
