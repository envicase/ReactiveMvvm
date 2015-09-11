using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace ReactiveMvvm
{
    /// <summary>
    /// <see cref="IObservable{T}"/> 인터페이스와
    /// <see cref="INotifyPropertyChanged"/> 인터페이스를 지원하는
    /// 메서드 집합을 제공합니다.
    /// </summary>
    public static class ObservableExtensions
    {
        private static string GetMemberName(Expression expression)
        {
            var body = expression as MemberExpression;
            if (body != null)
            {
                return body.Member.Name;
            }
            if (expression.NodeType == ExpressionType.Convert)
            {
                var unaryExpression = expression as UnaryExpression;
                body = unaryExpression.Operand as MemberExpression;
                if (body != null)
                {
                    return body.Member.Name;
                }
            }
            return null;
        }

        /// <summary>
        /// <see cref="INotifyPropertyChanged"/> 인터페이스 구현체의
        /// 지정한 속성을 관찰합니다.
        /// </summary>
        /// <typeparam name="TComponent">
        /// <see cref="INotifyPropertyChanged"/> 인터페이스 구현체 형식입니다.
        /// </typeparam>
        /// <typeparam name="TProperty">관찰할 속성의 형식입니다.</typeparam>
        /// <param name="component"><see cref="INotifyPropertyChanged"/>
        /// 인터페이스 구현체입니다.</param>
        /// <param name="propertyExpression">
        /// 관찰할 속성을 표현하는 식입니다.
        /// </param>
        /// <returns>
        /// <paramref name="propertyExpression"/>이 표현하는 속성 값의
        /// 변화를 노출하는 <see cref="IObservable{T}"/>입니다.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// 매개변수 propertyExpression이 올바른 속성 표현식이 아닌 경우
        /// </exception>
        public static IObservable<TProperty> Observe<TComponent, TProperty>(
            this TComponent component,
            Expression<Func<TComponent, TProperty>> propertyExpression)
            where TComponent : INotifyPropertyChanged
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }
            if (propertyExpression == null)
            {
                throw new ArgumentNullException(nameof(propertyExpression));
            }

            var propertyName = GetMemberName(propertyExpression.Body);
            if (propertyName == null)
            {
                throw new ArgumentException(
                    "The expression is not a property expression.",
                    paramName: nameof(propertyExpression));
            }

            var compiled = propertyExpression.Compile();
            var source = Observable.FromEventPattern<PropertyChangedEventArgs>(
                component, nameof(component.PropertyChanged));
            return from e in source
                   where e.EventArgs.PropertyName == propertyName
                   select compiled.Invoke(component);
        }
    }
}
