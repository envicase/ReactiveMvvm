using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using ReactiveMvvm.Models;
using Xunit;

namespace ReactiveMvvm.Tests.Models
{
    [ClearStreamAfterTest(typeof(User), typeof(string))]
    public class StreamTest
    {
        [Theory, AutoData]
        public void GetReturnsStreamForId(string id)
        {
            var stream = Stream<User, string>.Get(id);
            stream.Should().NotBeNull();
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
            var stream = Stream<User, string>.Get(user.Id);
            var actual = new List<User>();
            stream.Subscribe(u => actual.Add(u));
            stream.Subscribe(u => actual.Add(u));
            actual.Clear();

            stream.OnNext(user);

            actual.Should().Equal(user, user);
        }

        [Theory, AutoData]
        public void StreamSendsLastRevisionToNewObserver(User user)
        {
            var stream = Stream<User, string>.Get(user.Id);
            stream.OnNext(user);
            User actual = null;

            stream.Subscribe(u => actual = u);

            actual.Should().Be(user);
        }

        [Theory, AutoData]
        public void OnNextDoesNotSendModelSameAsLast(User user)
        {
            var stream = Stream<User, string>.Get(user.Id);
            stream.OnNext(user);
            User actual = null;
            stream.Subscribe(u => actual = u);
            actual = null;

            stream.OnNext(user);

            actual.Should().BeNull();
        }

        [Theory, AutoData]
        public void OnNextDoesNotSendModelEqualToLast(User user)
        {
            Stream<User, string>.EqualityComparer =
                Mock.Of<IEqualityComparer<User>>(
                    c => c.Equals(It.IsAny<User>(), It.IsAny<User>()) == true);
            var stream = Stream<User, string>.Get(user.Id);
            stream.OnNext(user);
            User actual = null;
            stream.Subscribe(u => actual = u);
            actual = null;

            stream.OnNext(new User(user.Id, user.UserName, user.Bio));

            actual.Should().BeNull();
        }
    }
}
