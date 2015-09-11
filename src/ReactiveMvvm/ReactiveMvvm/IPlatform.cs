using System;
using System.Reactive.Concurrency;

namespace ReactiveMvvm
{
    /// <summary>
    /// 플랫폼 서비스를 정의합니다.
    /// </summary>
    public interface IPlatform
    {
        /// <summary>
        /// 주 스레드 스케줄러를 가져옵니다.
        /// </summary>
        IScheduler MainThreadScheduler { get; }
    }
}
