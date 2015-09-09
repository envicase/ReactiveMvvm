using System;

namespace ReactiveMvvm
{
    /// <summary>
    /// 반응형 스트림에 의해 제공되는 모델을 정의합니다.
    /// </summary>
    /// <typeparam name="TId">모델 식별자 형식입니다.</typeparam>
    public interface IModel<TId>
        where TId : IEquatable<TId>
    {
        /// <summary>
        /// 모델 식별자를 가져옵니다.
        /// </summary>
        TId Id { get; }
    }
}
