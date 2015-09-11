using System;

namespace ReactiveMvvm
{
    /// <summary>
    /// 의존성 서비스를 제공하는 기능을 정의합니다.
    /// </summary>
    public interface IServiceResolver : IServiceProvider
    {
        /// <summary>
        /// 지정한 형식의 서비스 개체를 가져옵니다.
        /// </summary>
        /// <typeparam name="T">가져올 서비스 형식입니다.</typeparam>
        /// <returns>
        /// <typeparamref name="T"/> 형식의 서비스 개체 또는
        /// 서비스 개체가 없으면 <c>null</c>입니다.
        /// </returns>
        T GetService<T>() where T : class;
    }
}
