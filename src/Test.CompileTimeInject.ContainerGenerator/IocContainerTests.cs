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
    }
}
