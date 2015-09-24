using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FluentAssertions;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace ReactiveMvvm.Tests
{
    [Collection("Using Stream<User, string>")]
    [ClearStreamAfterTest(typeof(User), typeof(string))]
    public class StreamConnectionTest
    {
        [Theory, AutoData]
        public void SubscribesStreamWeak(User user)
        {
            var stream = Stream<User, string>.Get(user.Id);
            var subject = new Subject<User>();
            var subjectReference = new WeakReference(subject);
            var sut = new StreamConnection<User, string>(
                user.Id, subject.OnNext);
            var sutReference = new WeakReference(sut);

            subject = null;
            sut = null;
            GC.Collect();

            subjectReference.IsAlive.Should().BeFalse();
            sutReference.IsAlive.Should().BeFalse();
        }

        [Theory, AutoData]
        public void RelaysOnNext(User user)
        {
            var functor = Mock.Of<IFunctor>();
            var sut = new StreamConnection<User, string>(
                user.Id, functor.Action);

            Stream<User, string>.Get(user.Id).OnNext(Observable.Return(user));

            Mock.Get(functor).Verify(f => f.Action(user), Times.Once());
        }
    }
}
