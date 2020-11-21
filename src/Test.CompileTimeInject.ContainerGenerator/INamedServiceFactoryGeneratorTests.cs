namespace CustomCode.CompileTimeInject.ContainerGenerator.Tests
{

    using Extensions;
    using Microsoft.CodeAnalysis.CSharp;
    using Syntax;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Xunit;

    /// <summary>
    /// Automated tests for the <see cref="INamedServiceFactoryGenerator"/> type.
    /// </summary>
    public sealed partial class INamedServiceFactoryGeneratorTests
    {
        [Fact]
        public void Test()
        {
            var d1 = new ConcurrentDictionary<Type, object>();
            var key = typeof(int);
            for (var i = 0; i < 1000000; ++i)
            {
                var value = d1.GetOrAdd(key, _ => 1);
            }

            var d3 = new Cache();
            for (var i = 0; i < 1000000; ++i)
            {
                var value = d3.GetOrAdd(key, _ => 1);
            }

            var d4 = new Cache2();
            for (var i = 0; i < 1000000; ++i)
            {
                var value = d4.GetOrAdd(key, _ => Guid.NewGuid());
            }

            var d2 = new Dictionary<string, object>();
            d2.Add(typeof(int).AssemblyQualifiedName, 1);
            d2.Add(typeof(string).AssemblyQualifiedName, "awd");
            d2.Add(typeof(byte).AssemblyQualifiedName, 2);
            d2.Add(typeof(long).AssemblyQualifiedName, 3);
            d2.Add(typeof(short).AssemblyQualifiedName, 4);

            var key2 = typeof(int).AssemblyQualifiedName;
            for (var i = 0; i < 1000000; ++i)
            {
                var value = d2[key2];
            }
        }

        [Fact(DisplayName = "Class : IFoo (transient)")]
        public void GenerateServiceFactoryForNamedServices()
        {
            // Given
            var input = CompilationBuilder.CreateAssemblyWithCode(
                @"namespace Demo.Domain
                  {
                      public interface IFoo
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export(ServiceId = ""First"")]
                      public sealed class FirstFoo : IFoo
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export(ServiceId = ""Second"")]
                      public sealed class SecondFoo : IFoo
                      { }
                  }");
            var sourceGenerator = new INamedServiceFactoryGenerator();
            var testEnvironment = CSharpGeneratorDriver.Create(sourceGenerator);

            // When
            testEnvironment.RunGeneratorsAndUpdateCompilation(
                compilation: input,
                outputCompilation: out var output,
                diagnostics: out var diagnostics);

            // Then
            Assert.False(diagnostics.HasErrors());
            Assert.True(output.ContainsTypeWithMethodImplementation(
                "ServiceFactory",
               @"Demo.Domain.IFoo IServiceFactory<Demo.Domain.IFoo>.CreateOrGetService()
                 {
                     var service = new Demo.Domain.Foo();
                     return service;
                 }"));
        }
    }

    /// <summary>
    /// Specialized <see cref="ConcurrentDictionary{TKey, TValue}"/> that is used to store
    /// created singleton or scoped service instances.
    /// </summary>
    public sealed class Cache : ConcurrentDictionary<Type, object>
    { }

    public sealed class Cache2 : ConcurrentDictionary<Type, object>
    {
        private ConcurrentDictionary<Type, ConcurrentDictionary<string, object>> Cache { get; }
            = new ConcurrentDictionary<Type, ConcurrentDictionary<string, object>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="serviceId"></param>
        /// <param name="valueFactory"></param>
        /// <returns></returns>
        public object GetOrAdd(Type key, string serviceId, Func<string, object> valueFactory)
        {
            var typeCache = Cache.GetOrAdd(key, new ConcurrentDictionary<string, object>());
            return typeCache.GetOrAdd(serviceId, valueFactory);
        }
    }

    public interface IFoo
    { }

    public sealed class FirstFoo : IFoo
    { }

    public sealed class SecondFoo : IFoo
    { }

    public interface INamedServiceFactory<T> where T : class
    {
        T CreateOrGetNamedService(string serviceId);
    }

    public interface IServiceFactory<T> where T : class
    {
        T CreateOrGetService();
    }

    public sealed class ServiceFactory
        : INamedServiceFactory<IFoo>
        , IServiceFactory<IEnumerable<IFoo>>
    {
        public IFoo CreateOrGetNamedService(string serviceId)
        {
            var singletonInstances = new Cache2();
            var foo = (IFoo)singletonInstances.GetOrAdd(typeof(IFoo), serviceId, id =>
                {
                    if (id == "")
                    {
                        var foo = (IFoo)SingletonInstances.GetOrAdd(new ServiceKey(typeof(IFoo), serviceId), _ => new FirstFoo());
                        return foo;
                    }

                    if (id == "")
                    {
                        var foo = (IFoo)SingletonInstances.GetOrAdd(new ServiceKey(typeof(IFoo), serviceId), _ => new SecondFoo());
                        return foo;
                    }
                    throw new NotSupportedException("");
                });
            return foo;
        }

        public IEnumerable<IFoo> CreateOrGetService()
        {
            throw new NotImplementedException();
        }

        private ConcurrentDictionary<ServiceKey, object> SingletonInstances { get; }

        public struct ServiceKey
        {
            public ServiceKey(Type type, string? serviceId = null)
            {
                Type = type;
                ServiceId = serviceId;
            }

            private Type Type { get; }

            private string? ServiceId { get; }

            public override int GetHashCode()
            {
                var hashCode = Type.GetHashCode();
                if (ServiceId != null)
                {
                    hashCode = hashCode * 7 + ServiceId.GetHashCode();
                }
                return hashCode;
            }
        }
    }
}
