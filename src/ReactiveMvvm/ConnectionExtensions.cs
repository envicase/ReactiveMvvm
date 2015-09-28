using System;
using System.Reactive.Linq;

namespace ReactiveMvvm
{
    /// <summary>
    /// <see cref="IConnection{TModel, TId}"/> 인터페이스를 지원하는
    /// 확장 메서드 집합을 제공합니다.
    /// </summary>
    public static class ConnectionExtensions
    {
        /// <summary>
        /// 지정한 스트림 연결을 통해 새 모델 개정 인스턴스를 방출합니다.
        /// </summary>
        /// <typeparam name="TModel">모델 형식입니다.</typeparam>
        /// <typeparam name="TId">모델 식별자 형식입니다.</typeparam>
        /// <param name="connection">모델 스트림 연결입니다.</param>
        /// <param name="model">모델 개정 인스턴스입니다.</param>
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
