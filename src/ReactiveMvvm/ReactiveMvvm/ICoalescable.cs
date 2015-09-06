using System;

namespace ReactiveMvvm
{
    /// <summary>
    /// 다른 개체와 상태를 유착하는 기능을 정의합니다.
    /// </summary>
    /// <typeparam name="T">상태를 유착할 개체의 형식입니다.</typeparam>
    public interface ICoalescable<T>
        where T : class
    {
        /// <summary>
        /// 이 개체와 지정한 개체의 상태를 유착 결과를 반환합니다.
        /// </summary>
        /// <param name="right">이 개체와 상태를 유착할 개체입니다.</param>
        /// <returns>
        /// 이 개체와 <paramref name="right"/>의 상태를 유착한 결과입니다.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// 매개변수 <paramref name="right"/>가 <c>null</c>입니다.
        /// </exception>
        T Coalesce(T right);
    }
}
