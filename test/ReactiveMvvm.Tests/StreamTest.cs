using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace ReactiveMvvm.Tests
{
    [Collection("Using Stream<User, string>")]
    [ClearStreamAfterTest(typeof(User), typeof(string))]
    public class StreamTest
    {
        [Theory, AutoData]
        public void GetReturnsStreamForId(string id)
        {
            var stream = Stream<User, string>.Get(id);
            stream.Should().NotBeNull();
            stream.Id.Should().Be(id);
        }

        [Theory, AutoData]
        public void GetReturnsSameStreamInstanceForSameId(string id)
        {
            var expected = Stream<User, string>.Get(id);
            var actual = Stream<User, string>.Get(id);
            actual.Should().BeSameAs(expected);
        }

        [Theory, AutoData]
        public async Task GetShouldBeThreadSafe(string id)
        {
            var results = await Task.WhenAll(
                from _ in Enumerable.Range(0, Environment.ProcessorCount)
                select Task.Factory.StartNew(
                    () => Stream<User, string>.Get(id)));

            foreach (var r in results.Skip(1))
            {
                r.Should().BeSameAs(results[0]);
            }
        }

        [Theory, AutoData]
        public void OnNextSendsModelToAllObservers(User user)
        {
            var sut = Stream<User, string>.Get(user.Id);
            var actual = new List<User>();
            sut.Subscribe(u => actual.Add(u));
            sut.Subscribe(u => actual.Add(u));
            actual.Clear();

            sut.OnNext(Observable.Return(user));

            actual.Should().Equal(user, user);
        }

        [Theory, AutoData]
        public void StreamSendsLastRevisionToNewObserver(User user)
        {
            var sut = Stream<User, string>.Get(user.Id);
            sut.OnNext(Observable.Return(user));
            User actual = null;

            sut.Subscribe(u => actual = u);

            actual.Should().Be(user);
        }

        [Theory, AutoData]
        public void OnNextDoesNotSendModelSameAsLast(User user)
        {
            var sut = Stream<User, string>.Get(user.Id);
            sut.OnNext(Observable.Return(user));
            User actual = null;
            sut.Subscribe(u => actual = u);
            actual = null;

            sut.OnNext(Observable.Return(user));

            actual.Should().BeNull();
        }

        [Theory, AutoData]
        public void OnNextDoesNotSendModelEqualToLast(User user)
        {
            Stream<User, string>.EqualityComparer =
                Mock.Of<IEqualityComparer<User>>(
                    c => c.Equals(It.IsAny<User>(), It.IsAny<User>()) == true);
            var sut = Stream<User, string>.Get(user.Id);
            sut.OnNext(Observable.Return(user));
            User actual = null;
            sut.Subscribe(u => actual = u);
            actual = null;

            sut.OnNext(Observable.Return(
                new User(user.Id, user.UserName, user.Bio)));

            actual.Should().BeNull();
        }

        [Theory, AutoData]
        public async Task OnNextSwitchesWithObservable(User user, string bio)
        {
            var sut = Stream<User, string>.Get(user.Id);
            User actual = null;
            sut.Subscribe(u => actual = u);
            var task = Task.Delay(10).ContinueWith(_ => user);

            sut.OnNext(task.ToObservable());
            sut.OnNext(Observable.Return(
                new User(user.Id, user.UserName, bio)));
            await task;
            await Task.Delay(10);

            actual.Should().NotBeNull();
            actual.Bio.Should().Be(bio);
        }

        [Theory, AutoData]
        public void ClearRemovesAndDisposesAllStreams(User user)
        {
            var stream = Stream<User, string>.Get(user.Id);
            Action action = () => stream.OnNext(Observable.Return(user));

            Stream<User, string>.Clear();

            Stream<User, string>.Get(user.Id).Should().NotBeSameAs(stream);
            action.ShouldThrow<InvalidOperationException>();
        }
    }
}
