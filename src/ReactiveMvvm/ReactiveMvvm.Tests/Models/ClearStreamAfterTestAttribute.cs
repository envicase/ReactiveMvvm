using System;
using System.Reflection;
using ReactiveMvvm.Models;
using Xunit.Sdk;

namespace ReactiveMvvm.Tests.Models
{
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Method,
        AllowMultiple = true,
        Inherited = true)]
    public class ClearStreamAfterTestAttribute : BeforeAfterTestAttribute
    {
        public ClearStreamAfterTestAttribute(Type modelType, Type idType)
        {
            if (modelType == null)
            {
                throw new ArgumentNullException(nameof(modelType));
            }
            if (idType == null)
            {
                throw new ArgumentNullException(nameof(idType));
            }

            ModelType = modelType;
            IdType = idType;
        }

        public Type ModelType { get; }

        public Type IdType { get; }

        public override void After(MethodInfo methodUnderTest)
        {
            base.After(methodUnderTest);

            var type = typeof(Stream<,>).MakeGenericType(ModelType, IdType);
            type.GetProperty(nameof(Stream<User, string>.EqualityComparer))
                .SetValue(null, null);
            type.GetProperty(nameof(Stream<User, string>.Coalescer))
                .SetValue(null, null);
            type.GetMethod("Clear").Invoke(null, null);
        }
    }
}
