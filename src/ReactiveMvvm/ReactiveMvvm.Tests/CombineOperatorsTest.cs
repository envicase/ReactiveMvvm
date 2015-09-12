using System;
using FluentAssertions;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace ReactiveMvvm.Tests
{
    public class CombineOperatorsTest
    {
        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(false, false, false)]
        public void OrWith2ArgsReturnsCorrectValue(
            bool arg1, bool arg2, bool expected)
        {
            CombineOperators.Or(arg1, arg2).Should().Be(expected);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(false, false, false)]
        public void OrWith3ArgsReturnsCorrectValue(
            bool repeat, bool last, bool expected)
        {
            CombineOperators.Or(repeat, repeat, last).Should().Be(expected);
        }

        [Theory, AutoData]
        [InlineData(true, true, true)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(false, false, false)]
        public void OrWith4ArgsReturnsCorrectValue(
            bool repeat, bool last, bool expected)
        {
            CombineOperators.Or(repeat, repeat, repeat, last)
                .Should().Be(expected);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        public void AndWith2ArgsReturnsCorrectValue(
            bool arg1, bool arg2, bool expected)
        {
            CombineOperators.And(arg1, arg2).Should().Be(expected);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        public void AndWith3ArgsReturnsCorrectValue(
            bool repeat, bool last, bool expected)
        {
            CombineOperators.And(repeat, repeat, last).Should().Be(expected);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        public void AndWith4ArgsReturnsCorrectValue(
            bool repeat, bool last, bool expected)
        {
            CombineOperators.And(repeat, repeat, repeat, last)
                .Should().Be(expected);
        }
    }
}
