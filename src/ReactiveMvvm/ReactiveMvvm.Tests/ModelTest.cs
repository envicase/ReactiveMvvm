using System;
using FluentAssertions;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace ReactiveMvvm.Tests
{
    public class ModelTest
    {
        [Theory, AutoData]
        public void SetIdCorrectly(string id, string userName, string bio)
        {
            var user = new User(id, userName, bio);
            user.Id.Should().BeSameAs(id);
        }
    }
}
