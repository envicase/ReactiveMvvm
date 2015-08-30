using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Ploeh.AutoFixture.Xunit2;
using ReactiveMvvm.Models;
using Xunit;

namespace ReactiveMvvm.Tests.Models
{
    public class StreamStoreTest
    {
        [Theory, AutoData]
        public void ProvidesStreamForId(string id)
        {
            var stream = StreamStore<User, string>.GetStream(id);
            stream.Should().NotBeNull();
        }

        [Theory, AutoData]
        public void ProvidesSameStreamInstanceForSameId(string id)
        {
            var expected = StreamStore<User, string>.GetStream(id);
            var actual = StreamStore<User, string>.GetStream(id);
            actual.Should().BeSameAs(expected);
        }

        [Theory, AutoData]
        public async Task GetStreamShouldBeThreadSafe(string id)
        {
            var results = await Task.WhenAll(
                from _ in Enumerable.Range(0, Environment.ProcessorCount)
                select Task.Factory.StartNew(
                    () => StreamStore<User, string>.GetStream(id)));

            foreach (var r in results.Skip(1))
            {
                r.Should().BeSameAs(results[0]);
            }
        }

        [Theory, AutoData]
        public void PushSendsModelToAllObservers(User user)
        {
            var stream = StreamStore<User, string>.GetStream(user.Id);
            var actual = new List<User>();
            stream.Subscribe(u => actual.Add(u));
            stream.Subscribe(u => actual.Add(u));
            actual.Clear();

            StreamStore<User, string>.Push(user);

            actual.Should().Equal(user, user);
        }

        [Theory, AutoData]
        public void StreamSendsModelToNewObserver(User user)
        {
            StreamStore<User, string>.Push(user);
            User actual = null;
            var stream = StreamStore<User, string>.GetStream(user.Id);

            stream.Subscribe(u => actual = u);

            actual.Should().Be(user);
        }

        [Theory, AutoData]
        public void PushDoesNotSendModelSameAsLast(User user)
        {
            StreamStore<User, string>.Push(user);
            User actual = null;
            StreamStore<User, string>
                .GetStream(user.Id).Subscribe(u => actual = u);
            actual = null;

            StreamStore<User, string>.Push(user);

            actual.Should().BeNull();
        }

        [Theory, AutoData]
        public void PushDoesNotSendModelEqualToLast(
            User user, UserEqualityComparer comparer)
        {
            StreamStore<User, string>.EqualityComparer = comparer;
            StreamStore<User, string>.Push(user);
            User actual = null;
            StreamStore<User, string>
                .GetStream(user.Id).Subscribe(u => actual = u);
            actual = null;

            StreamStore<User, string>.Push(
                new User(user.Id, user.UserName, user.Bio));

            actual.Should().BeNull();
        }
    }
}
