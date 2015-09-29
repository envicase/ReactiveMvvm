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
    using static It;
    using static Stream<User, string>;
    using static Times;

    [Collection("Using Stream<User, string>")]
    [ClearStreamAfterTest(typeof(User), typeof(string))]
    public class StreamTest
    {
        [Theory, AutoData]
        public void ExistsForReturnsFalseForUnconnected(string id) =>
            ExistsFor(id).Should().BeFalse();

        [Theory, AutoData]
        public void ExistsForReturnsTrueForConnected(string id)
        {
            IConnection<User, string> connection = Connect(id);
            bool actual = ExistsFor(id);
            actual.Should().BeTrue();
        }

        [Theory, AutoData]
        public void ClearRemovesAllStreams(List<string> ids)
        {
            List<IConnection<User, string>> connections =
                ids.Select(id => Connect(id)).ToList();

            Clear();

            ids.TrueForAll(id => false == ExistsFor(id)).Should().BeTrue();
        }

        [Theory, AutoData]
        public void ConnectReturnsConnection(string id)
        {
            IConnection<User, string> connection = Connect(id);

            connection.Should().NotBeNull();
            connection.ModelId.Should().Be(id);
        }

        [Theory, AutoData]
        public void ConnectionEmitPublishesModel(User user)
        {
            IConnection<User, string> connection = Connect(user.Id);
            var observer = Mock.Of<IObserver<User>>();
            connection.Subscribe(observer);

            Connect(user.Id).Emit(user);

            Mock.Get(observer).Verify(x => x.OnNext(user), Once());
        }

        [Theory, AutoData]
        public void ConnectionSendsLastToNewObserver(User user)
        {
            IConnection<User, string> connection = Connect(user.Id);
            connection.Emit(user);
            var observer = Mock.Of<IObserver<User>>();

            connection.Subscribe(observer);

            Mock.Get(observer).Verify(x => x.OnNext(user), Once());
        }

        [Theory, AutoData]
        public void NewConnectionSendsLastToObserver(User user)
        {
            Connect(user.Id).Emit(user);
            IConnection<User, string> connection = Connect(user.Id);
            var observer = Mock.Of<IObserver<User>>();

            connection.Subscribe(observer);

            Mock.Get(observer).Verify(x => x.OnNext(user), Once());
        }

        [Theory, AutoData]
        public void RemovesStreamThatHasNoConnection(string id)
        {
            List<IConnection<User, string>> connections =
                Enumerable.Repeat(id, 10).Select(Connect).ToList();

            connections.ForEach(x => x.Dispose());

            ExistsFor(id).Should().BeFalse();
        }

        [Theory, AutoData]
        public void EmitInterceptsModelSameAsLast(User user)
        {
            IConnection<User, string> connection = Connect(user.Id);
            var observer = Mock.Of<IObserver<User>>();
            connection.Emit(user);
            connection.Subscribe(observer);
            Mock.Get(observer).Verify(x => x.OnNext(user), Once());

            connection.Emit(user);

            Mock.Get(observer).Verify(x => x.OnNext(user), Once());
        }

        [Theory, AutoData]
        public void EmitinterceptsModelEqualToLast(
            User user, string name, string bio)
        {
            EqualityComparer = Mock.Of<IEqualityComparer<User>>(
                x => x.Equals(IsNotNull<User>(), IsNotNull<User>()) == true);
            IConnection<User, string> connection = Connect(user.Id);
            var observer = Mock.Of<IObserver<User>>();
            connection.Emit(user);
            connection.Subscribe(observer);
            Mock.Get(observer).Verify(x => x.OnNext(IsAny<User>()), Once());

            connection.Emit(new User(user.Id, name, bio));

            Mock.Get(observer).Verify(x => x.OnNext(IsAny<User>()), Once());
        }

        [Theory, AutoData]
        public async Task EmitUnsubscribesPreviouslyEmitted(
            User user, string name, string bio)
        {
            IConnection<User, string> connection = Connect(user.Id);
            var observer = Mock.Of<IObserver<User>>();
            connection.Subscribe(observer);
            Task<User> task = Task.Delay(10).ContinueWith(_ => user);

            connection.Emit(task.ToObservable());
            connection.Emit(Observable.Return(new User(user.Id, name, bio)));
            await task;
            await Task.Delay(10);

            Mock.Get(observer).Verify(x => x.OnNext(user), Never());
            Mock.Get(observer).Verify(x => x.OnNext(
                Is<User>(p => p.UserName == name && p.Bio == bio)), Once());
        }
    }
}
