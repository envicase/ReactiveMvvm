using System;
using System.Reactive.Linq;

namespace ReactiveMvvm
{
    // TODO: StreamExtensions 클래스의 디자인이 성숙 단계에 이르면
    // XML 주석을 추가하고 아래 pragma 지시문을 제거합니다.
#pragma warning disable CS1591

    public static class StreamExtensions
    {
        public static void OnNext<TModel, TId>(
            this Stream<TModel, TId> stream, TModel value)
            where TModel : class, IModel<TId>
            where TId : IEquatable<TId>
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            stream.OnNext(Observable.Return(value));
        }
    }
}
