using System;

namespace ReactiveMvvm
{
    /// <summary>
    /// 반응형 스트림에 의해 제공되는 모델의 추상 기반 클래스를 제공합니다.
    /// </summary>
    /// <typeparam name="TId">모델 식별자 형식입니다.</typeparam>
    public abstract class Model<TId>
        where TId : IEquatable<TId>
    {
        /// <summary>
        /// 모델 식별자를 사용하여 <see cref="Model{TId}"/> 클래스의
        /// 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="id">모델 식별자입니다.</param>
        /// <exception cref="ArgumentNullException">
        /// 매개변수 <paramref name="id"/>가 <c>null</c>인 경우
        /// </exception>
        protected Model(TId id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            Id = id;
        }

        /// <summary>
        /// 모델 식별자를 가져옵니다.
        /// </summary>
        public TId Id { get; }
    }
}
