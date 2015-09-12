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
        [InlineData(true, true, true, true)]
        [InlineData(true, true, false, true)]
        [InlineData(true, false, true, true)]
        [InlineData(true, false, false, true)]
        [InlineData(false, true, true, true)]
        [InlineData(false, true, false, true)]
        [InlineData(false, false, true, true)]
        [InlineData(false, false, false, false)]
        public void OrWith3ArgsReturnsCorrectValue(
            bool arg1, bool arg2, bool arg3, bool expected)
        {
            CombineOperators.Or(arg1, arg2, arg3).Should().Be(expected);
        }

        [Theory, AutoData]
        public void OrWith4ArgsReturnsCorrectValue(
            bool arg1, bool arg2, bool arg3, bool arg4)
        {
            CombineOperators.Or(arg1, arg2, arg3, arg4).Should()
                .Be(arg1 || arg2 || arg3 || arg4);
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
        [InlineData(true, true, true, true)]
        [InlineData(true, true, false, false)]
        [InlineData(true, false, true, false)]
        [InlineData(true, false, false, false)]
        [InlineData(false, true, true, false)]
        [InlineData(false, true, false, false)]
        [InlineData(false, false, true, false)]
        [InlineData(false, false, false, false)]
        public void AndWith3ArgsReturnsCorrectValue(
            bool arg1, bool arg2, bool arg3, bool expected)
        {
            CombineOperators.And(arg1, arg2, arg3).Should().Be(expected);
        }

        [Theory, AutoData]
        public void AndWith4ArgsReturnsCorrectValue(
            bool arg1, bool arg2, bool arg3, bool arg4)
        {
            CombineOperators.And(arg1, arg2, arg3, arg4).Should()
                .Be(arg1 && arg2 && arg3 && arg4);
        }
    }
}
