using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ReactiveMvvm
{
    /// <summary>
    /// 반응형 명령을 정의합니다.
    /// </summary>
    public interface IReactiveCommand : ICommand, IDisposable
    {
        /// <summary>
        /// 비동기적으로 명령을 실행합니다.
        /// </summary>
        /// <param name="parameter">명령 매개변수입니다.</param>
        /// <returns>비동기 작업입니다.</returns>
        Task ExecuteAsync(object parameter);
    }

    /// <summary>
    /// 실행 결과를 관찰할 수 있는 반응형 명령을 정의합니다.
    /// </summary>
    /// <typeparam name="T">명령 실행 결과 형식입니다.</typeparam>
    public interface IReactiveCommand<T> : IReactiveCommand, IObservable<T>
    {
    }
}
