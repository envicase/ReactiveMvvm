using System;

namespace ReactiveMvvm
{
    // TODO: Locator 클래스 디자인이 성숙 단계에 이르면
    // XML 주석을 추가하고 아래 pragma 지시문을 삭제합니다.
#pragma warning disable CS1591

    public static class Resolver
    {
        private static Func<IServiceResolver> _provider;

        public static bool IsResolverProviderSet => _provider != null;

        public static IServiceResolver Instance
        {
            get
            {
                if (false == IsResolverProviderSet)
                {
                    throw new InvalidOperationException(
                        "Service resolver provider was not set.");
                }

                return _provider.Invoke();
            }
        }

        public static void SetResolverProvider(Func<IServiceResolver> provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            _provider = provider;
        }
    }
}
