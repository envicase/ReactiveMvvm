using System;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace ReactiveMvvm.Tests
{
    using static Stream<User, string>;
    using static Times;

    public class ConnectionExtensionsTest
    {
        [Theory AutoData]
        public void EmitRelaysWithModelInstance(User user)
        {
            IConnection<User, string> connection = Connect(user.Id);
            var observable = Mock.Of<IObserver<User>>();
            connection.Subscribe(observable);

            connection.Emit(user);

            Mock.Get(observable).Verify(x => x.OnNext(user), Once());
        }
    }
}
