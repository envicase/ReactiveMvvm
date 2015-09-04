using System;
using FluentAssertions;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace ReactiveMvvm.Tests
{
    public class DefaultCoalescerTest
    {
        [Theory, AutoData]
        public void CoalesceReturnsLeftForNonCoalescable(User left, User right)
        {
            var sut = Coalescer<User>.Default;
            var actual = sut.Coalesce(left, right);
            actual.Should().BeSameAs(left);
        }

        [Theory, AutoData]
        public void CoalesceDelegatesToCoalescable(UserCoalescable right)
        {
            var left = new UserCoalescable(right.Id, right.UserName, null);
            var sut = Coalescer<UserCoalescable>.Default;

            var actual = sut.Coalesce(left, right);

            actual.ShouldBeEquivalentTo(
                new { left.Id, left.UserName, right.Bio });
        }
    }
}
