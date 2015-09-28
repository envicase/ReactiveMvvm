using System;

namespace ReactiveMvvm
{
    /// <summary>
    /// 모델 스트림 연결을 정의합니다.
    /// </summary>
    /// <typeparam name="TModel">모델 형식입니다.</typeparam>
    /// <typeparam name="TId">모델 식별자 형식입니다.</typeparam>
    public interface IConnection<TModel, TId> : IObservable<TModel>, IDisposable
        where TModel : class, IModel<TId>
        where TId : IEquatable<TId>
    {
        /// <summary>
        /// 스트림에 의해 처리되는 모델 식별자를 가져옵니다.
        /// </summary>
        TId ModelId { get; }

        /// <summary>
        /// 새 모델 개정의 소스를 스트림에 전달합니다.
        /// </summary>
        /// <param name="source">모델 개정 소스입니다.</param>
        void Send(IObservable<TModel> source);
    }
}
