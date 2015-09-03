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
        public void OnNextSendModelToStream(User user)
        {
            var sut = new ModelSwitch<User, string>(user.Id);
            User actual = null;
            sut.Stream.Subscribe(u => actual = u);
            var observable = Task.FromResult(user).ToObservable();

            sut.OnNext(observable);

            actual.Should().BeSameAs(user);
        }
    }
}
