using System;

namespace ReactiveMvvm
{
    /// <summary>
    /// 두 개체의 상태를 유착하는 기능을 제공하는 메서드를 정의합니다.
    /// </summary>
    /// <typeparam name="T">상태를 유착할 개체의 형식입니다.</typeparam>
    public interface ICoalescer<T>
        where T : class
    {
        /// <summary>
        /// 두 개체 상태의 유착 결과를 반환합니다.
        /// </summary>
        /// <param name="left">
        /// 첫번째 피연산자입니다. 구현체는 이 연산자가 가진 상태가
        /// 우선 순위를 갖도록 기능을 제공합니다.
        /// </param>
        /// <param name="right">
        /// 두번째 피연산자입니다. 구현체는 이 연산자가 <paramref name="left"/>가
        /// 가지지 않은 상태에 대해 대체값을 제공하도록 기능을 제공합니다.
        /// </param>
        /// <returns><paramref name="left"/>와 <paramref name="right"/>의
        /// 상태를 유착한 결과입니다.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>매개변수 <paramref name="left"/>가 <c>null</c>인 경우</para>
        /// <para>- 또는 -</para>
        /// <para>매개변수 <paramref name="right"/>가 <c>null</c>인 경우</para>
        /// </exception>
        T Coalesce(T left, T right);
    }
}
