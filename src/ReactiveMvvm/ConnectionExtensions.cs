using System;
using System.Reactive.Linq;

namespace ReactiveMvvm
{
    public static class ConnectionExtensions
    {
        public static void Emit<TModel, TId>(
            this IConnection<TModel, TId> connection, TModel model)
            where TModel : class, IModel<TId>
            where TId : IEquatable<TId>
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            connection.Emit(Observable.Return(model));
        }
    }
}
