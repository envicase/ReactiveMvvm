using System;
using FluentAssertions;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace ReactiveMvvm.Tests
{
    public class ObservableExtensionsTest
    {
        public class Component : ObservableObject
        {
            private string _foo;

            public string Foo
            {
                get { return _foo; }
                set { SetValue(ref _foo, value); }
            }
        }

        [Theory, AutoData]
        public void ObserveReturnsObservableForSpecifiedProperty(
            string first, string second)
        {
            var component = new Component { Foo = first };
            var functor = Mock.Of<IFunctor>();
            component.Observe(x => x.Foo)?.Subscribe(functor.Action);

            component.Foo = second;

            Mock.Get(functor).Verify(f => f.Action(first), Times.Once());
            Mock.Get(functor).Verify(f => f.Action(second), Times.Once());
        }

        [Fact]
        public void ObserveFailsWithInvalidExpression()
        {
            var component = new Component();
            Action action = () => component.Observe(x => x.ToString());
            action.ShouldThrow<ArgumentException>();
        }
    }
}
