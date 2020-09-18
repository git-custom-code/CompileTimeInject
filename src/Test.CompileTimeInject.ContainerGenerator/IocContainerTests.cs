namespace CustomCode.CompileTimeInject.ContainerGenerator.Tests
{
    using GeneratedCode;
    using System.Linq;
    using Xunit;
    using directRef = Test.DirectReference;

    public sealed class IocContainerTests
    {
        [Fact]
        public void GetServiceByImplementation()
        {
            // Given
            var container = new IocContainer();

            // When
            var foo = container.GetService<directRef.ByImplementation.Foo>();

            // Then
            Assert.NotNull(foo);
        }

        [Fact]
        public void GetServiceBySingleContract()
        {
            // Given
            var container = new IocContainer();

            // When
            var fooByContract = container.GetService<directRef.BySingleContract.IFoo>();
            var fooByImplementation = container.GetService<directRef.BySingleContract.Foo>();

            // Then
            Assert.NotNull(fooByContract);
            Assert.Null(fooByImplementation);
        }

        [Fact]
        public void GetServiceByMultipleContracts()
        {
            // Given
            var container = new IocContainer();

            // When
            var fooByContract = container.GetService<directRef.ByMultipleContracts.IFoo>();
            var barByContract = container.GetService<directRef.ByMultipleContracts.IBar>();
            var fooByImplementation = container.GetService<directRef.ByMultipleContracts.Foo>();

            // Then
            Assert.NotNull(fooByContract);
            Assert.NotNull(barByContract);
            Assert.Null(fooByImplementation);
        }

        [Fact]
        public void GetServicesByMultipleImplementations()
        {
            // Given
            var container = new IocContainer();

            // When
            var singleFoo = container.GetService<directRef.ByMultipleImplementations.IFoo>();
            var fooCollection = container.GetServices<directRef.ByMultipleImplementations.IFoo>();

            // Then
            Assert.Null(singleFoo);
            Assert.NotNull(fooCollection);
            Assert.Equal(2, fooCollection.Count());
        }

        [Fact]
        public void GetServicesAsSingleton()
        {
            // Given
            var container = new IocContainer();

            // When
            var foo1 = container.GetService<directRef.AsSingleton.IFoo>();
            var foo2 = container.GetService<directRef.AsSingleton.IFoo>();

            // Then
            Assert.NotNull(foo1);
            Assert.NotNull(foo2);
            Assert.Equal(foo1, foo2);
            Assert.Equal(foo1?.Id, foo2?.Id);
        }

        [Fact]
        public void GetServicesByFilteredContract()
        {
            // Given
            var container = new IocContainer();

            // When
            var foo = container.GetService<directRef.ByFilteredContract.IFoo>();
            var bar = container.GetService<directRef.ByFilteredContract.IBar>();

            // Then
            Assert.Null(foo);
            Assert.NotNull(bar);
        }

        [Fact]
        public void GetServicesByGenericContract()
        {
            // Given
            var container = new IocContainer();

            // When
            var foo = container.GetService<directRef.ByGenericContract.IFoo<int[,]>>();

            // Then
            Assert.NotNull(foo);
        }

        [Fact]
        public void WithSingleDependency()
        {
            // Given
            var container = new IocContainer();

            // When
            var foo = container.GetService<directRef.WithSingleDependency.IFoo>();

            // Then
            Assert.NotNull(foo);
            Assert.NotNull(foo?.Dependency);
        }

        [Fact]
        public void WithMultipleDependencies()
        {
            // Given
            var container = new IocContainer();

            // When
            var foo = container.GetService<directRef.WithMultipleDependencies.IFoo>();

            // Then
            Assert.NotNull(foo);
            Assert.NotNull(foo?.TransientDependency);
            Assert.NotNull(foo?.SingletonDependency);
        }

        [Fact]
        public void WithCollectionDependency()
        {
            // Given
            var container = new IocContainer();

            // When
            var foo = container.GetService<directRef.WithCollectionDependency.IFoo>();

            // Then
            Assert.NotNull(foo);
            Assert.NotNull(foo?.Dependencies);
            Assert.Equal(2, foo?.Dependencies.Count());
        }

        [Fact]
        public void ByMultipleImplementationsWithDependencies()
        {
            // Given
            var container = new IocContainer();

            // When
            var fooCollection = container.GetServices<directRef.ByMultipleImplementationsWithDependencies.IFoo>();

            // Then
            Assert.NotNull(fooCollection);
            Assert.Equal(2, fooCollection.Count());
            foreach(var foo in fooCollection)
            {
                Assert.NotNull(foo);
                foreach(var dependency in foo.Dependencies)
                {
                    Assert.NotNull(dependency);
                }
            }
        }

        [Fact]
        public void WithFactoryDependency()
        {
            // Given
            var container = new IocContainer();

            // When
            var foo = container.GetService<directRef.WithFactoryDependency.IFoo>();

            // Then
            Assert.NotNull(foo);
            Assert.NotNull(foo?.Dependency);
        }
    }
}
