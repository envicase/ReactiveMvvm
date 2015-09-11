using System;
using FluentAssertions;
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
        public void ObserveReturnsObservableForSpecifiedProperty(string value)
        {
            var component = new Component();
            string actual = null;
            component.Observe(x => x.Foo)?.Subscribe(v => actual = v);

            component.Foo = value;

            actual.Should().BeSameAs(value);
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
