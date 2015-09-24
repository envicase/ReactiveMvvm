using System;
using FluentAssertions;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace ReactiveMvvm.Tests
{
    public class StreamExtensionsTest
    {
        [Theory AutoData]
        public void OnNextRelaysWithModelValue(User user)
        {
            var stream = Stream<User, string>.Get(user.Id);
            User actual = null;
            stream.Subscribe(u => actual = u);

            stream.OnNext(user);

            actual.Should().BeSameAs(user);
        }
    }
}
