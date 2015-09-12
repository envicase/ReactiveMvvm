using System;

namespace ReactiveMvvm
{
    // TODO: CombineOperators 클래스 디자인이 성숙단계에 이르면
    // XML 주석을 추가하고 아래 pragma 지시문을 삭제합니다.
#pragma warning disable CS1591

    public static class CombineOperators
    {
        public static bool Or(bool arg1, bool arg2) => arg1 || arg2;

        public static bool Or(bool arg1, bool arg2, bool arg3)
            => arg1 || arg2 || arg3;

        public static bool Or(bool arg1, bool arg2, bool arg3, bool arg4)
            => arg1 || arg2 || arg3 || arg4;

        public static bool And(bool arg1, bool arg2) => arg1 && arg2;

        public static bool And(bool arg1, bool arg2, bool arg3)
            => arg1 && arg2 && arg3;

        public static bool And(bool arg1, bool arg2, bool arg3, bool arg4)
            => arg1 && arg2 && arg3 && arg4;
    }
}
