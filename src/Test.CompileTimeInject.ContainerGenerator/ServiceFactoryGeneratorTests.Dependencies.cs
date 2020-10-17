namespace CustomCode.CompileTimeInject.ContainerGenerator.Tests
{
    using Extensions;
    using Microsoft.CodeAnalysis.CSharp;
    using Syntax;
    using Xunit;

    /// <summary>
    /// Automated tests for the <see cref="ServiceFactoryGenerator"/> type for dependency injection.
    /// </summary>
    public sealed partial class ServiceFactoryGeneratorTests
    {
        [Fact(DisplayName = "Class : IFoo (transient, with IBar)")]
        public void GenerateServiceFactoryForTransientClassWithSingleDependency()
        {
            // Given
            var input = CompilationBuilder.CreateAssemblyWithCode(
                @"namespace Demo.Domain
                  {
                      public interface IBar
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export]
                      public sealed class Bar : IBar
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      public interface IFoo
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export(Lifetime.Transient)]
                      public sealed class Foo : IFoo
                      {
                          public Foo(IBar bar)
                          {
                              Bar = bar;
                          }

                          private IBar Bar { get; }
                      }
                  }");
            var sourceGenerator = new ServiceFactoryGenerator();
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
                     var dependency1 = ((IServiceFactory<Demo.Domain.IBar>)this).CreateOrGetService();
                     var service = new Demo.Domain.Foo(dependency1);
                     return service;
                 }"));
        }

        [Fact(DisplayName = "Class : IFoo (transient, with IFirstBar, ISecondBar)")]
        public void GenerateServiceFactoryForTransientClassWithTwoDependencies()
        {
            // Given
            var input = CompilationBuilder.CreateAssemblyWithCode(
                @"namespace Demo.Domain
                  {
                      public interface IFirstBar
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export]
                      public sealed class FirstBar : IFirstBar
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      public interface ISecondBar
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export]
                      public sealed class SecondBar : ISecondBar
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      public interface IFoo
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export(Lifetime.Transient)]
                      public sealed class Foo : IFoo
                      {
                          public Foo(IFirstBar firstBar, ISecondBar secondBar)
                          {
                              FirstBar = firstBar;
                              SecondBar = secondBar;
                          }

                          private IFirstBar FirstBar { get; }

                          private ISecondBar SecondBar { get; }
                      }
                  }");
            var sourceGenerator = new ServiceFactoryGenerator();
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
                     var dependency1 = ((IServiceFactory<Demo.Domain.IFirstBar>)this).CreateOrGetService();
                     var dependency2 = ((IServiceFactory<Demo.Domain.ISecondBar>)this).CreateOrGetService();
                     var service = new Demo.Domain.Foo(dependency1, dependency2);
                     return service;
                 }"));
        }

        [Fact(DisplayName = "Class : IFoo (transient, with IEnumerable<IBar>)")]
        public void GenerateServiceFactoryForTransientClassWithDependencyCollection()
        {
            // Given
            var input = CompilationBuilder.CreateAssemblyWithCode(
                @"namespace Demo.Domain
                  {
                      public interface IBar
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export]
                      public sealed class FirstBar : IBar
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export]
                      public sealed class SecondBar : IBar
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      public interface IFoo
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;
                      using System;

                      [Export(Lifetime.Transient)]
                      public sealed class Foo : IFoo
                      {
                          public Foo(IEnumerable<IBar> bar)
                          {
                              Bar = bar;
                          }

                          private IEnumerable<IBar> Bar { get; }
                      }
                  }");
            var sourceGenerator = new ServiceFactoryGenerator();
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
                     var dependency1 = ((IServiceFactory<IEnumerable<Demo.Domain.IBar>>)this).CreateOrGetService();
                     var service = new Demo.Domain.Foo(dependency1);
                     return service;
                 }"));
        }

        [Fact(DisplayName = "Class : IFoo (transient, with Func<IBar>)")]
        public void GenerateServiceFactoryForTransientClassWithSingleDependencyFactory()
        {
            // Given
            var input = CompilationBuilder.CreateAssemblyWithCode(
                @"namespace Demo.Domain
                  {
                      public interface IBar
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export]
                      public sealed class Bar : IBar
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      public interface IFoo
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;
                      using System;

                      [Export(Lifetime.Transient)]
                      public sealed class Foo : IFoo
                      {
                          public Foo(Func<IBar> barFactory)
                          {
                              BarFactory = barFactory;
                          }

                          private Func<IBar> BarFactory { get; }
                      }
                  }");
            var sourceGenerator = new ServiceFactoryGenerator();
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
                     var dependency1 = new Func<Demo.Domain.IBar>(((IServiceFactory<Demo.Domain.IBar>)this).CreateOrGetService);
                     var service = new Demo.Domain.Foo(dependency1);
                     return service;
                 }"));
        }

        [Fact(DisplayName = "Class : IFoo (singleton, with IBar)")]
        public void GenerateServiceFactoryForSingletonClassWithSingleDependency()
        {
            // Given
            var input = CompilationBuilder.CreateAssemblyWithCode(
                @"namespace Demo.Domain
                  {
                      public interface IBar
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export]
                      public sealed class Bar : IBar
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      public interface IFoo
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export(Lifetime.Singleton)]
                      public sealed class Foo : IFoo
                      {
                          public Foo(IBar bar)
                          {
                              Bar = bar;
                          }

                          private IBar Bar { get; }
                      }
                  }");
            var sourceGenerator = new ServiceFactoryGenerator();
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
                     var service = (Demo.Domain.IFoo)SingletonInstances.GetOrAdd(typeof(Demo.Domain.IFoo), _ =>
                         {
                             var dependency1 = ((IServiceFactory<Demo.Domain.IBar>)this).CreateOrGetService();
                             var service = new Demo.Domain.Foo(dependency1);
                             return service;
                         });
                    return service;
                 }"));
        }

        [Fact(DisplayName = "Class : IFoo (singleton, with IFirstBar, ISecondBar)")]
        public void GenerateServiceFactoryForSingletonClassWithTwoDependencies()
        {
            // Given
            var input = CompilationBuilder.CreateAssemblyWithCode(
                @"namespace Demo.Domain
                  {
                      public interface IFirstBar
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export]
                      public sealed class FirstBar : IFirstBar
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      public interface ISecondBar
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export]
                      public sealed class SecondBar : ISecondBar
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      public interface IFoo
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export(Lifetime.Singleton)]
                      public sealed class Foo : IFoo
                      {
                          public Foo(IFirstBar firstBar, ISecondBar secondBar)
                          {
                              FirstBar = firstBar;
                              SecondBar = secondBar;
                          }

                          private IFirstBar FirstBar { get; }

                          private ISecondBar SecondBar { get; }
                      }
                  }");
            var sourceGenerator = new ServiceFactoryGenerator();
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
                     var service = (Demo.Domain.IFoo)SingletonInstances.GetOrAdd(typeof(Demo.Domain.IFoo), _ =>
                         {
                             var dependency1 = ((IServiceFactory<Demo.Domain.IFirstBar>)this).CreateOrGetService();
                             var dependency2 = ((IServiceFactory<Demo.Domain.ISecondBar>)this).CreateOrGetService();
                             var service = new Demo.Domain.Foo(dependency1, dependency2);
                             return service;
                         });
                    return service;
                 }"));
        }

        [Fact(DisplayName = "Class : IFoo (singleton, with IEnumerable<IBar>)")]
        public void GenerateServiceFactoryForSingletonClassWithDependencyCollection()
        {
            // Given
            var input = CompilationBuilder.CreateAssemblyWithCode(
                @"namespace Demo.Domain
                  {
                      public interface IBar
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export]
                      public sealed class FirstBar : IBar
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export]
                      public sealed class SecondBar : IBar
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      public interface IFoo
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export(Lifetime.Singleton)]
                      public sealed class Foo : IFoo
                      {
                          public Foo(IEnumerable<IBar> bar)
                          {
                              Bar = bar;
                          }

                          private IEnumerable<IBar> Bar { get; }
                      }
                  }");
            var sourceGenerator = new ServiceFactoryGenerator();
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
                     var service = (Demo.Domain.IFoo)SingletonInstances.GetOrAdd(typeof(Demo.Domain.IFoo), _ =>
                         {
                             var dependency1 = ((IServiceFactory<IEnumerable<Demo.Domain.IBar>>)this).CreateOrGetService();
                             var service = new Demo.Domain.Foo(dependency1);
                             return service;
                         });
                    return service;
                 }"));
        }

        [Fact(DisplayName = "Class : IFoo (singleton, with Func<IBar>)")]
        public void GenerateServiceFactoryForSingletonClassWithSingleDependencyFactory()
        {
            // Given
            var input = CompilationBuilder.CreateAssemblyWithCode(
                @"namespace Demo.Domain
                  {
                      public interface IBar
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;

                      [Export]
                      public sealed class Bar : IBar
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      public interface IFoo
                      { }
                  }",
                @"namespace Demo.Domain
                  {
                      using CustomCode.CompileTimeInject.Annotations;
                      using System;

                      [Export(Lifetime.Singleton)]
                      public sealed class Foo : IFoo
                      {
                          public Foo(Func<IBar> barFactory)
                          {
                              BarFactory = barFactory;
                          }

                          private Func<IBar> BarFactory { get; }
                      }
                  }");
            var sourceGenerator = new ServiceFactoryGenerator();
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
                     var service = (Demo.Domain.IFoo)SingletonInstances.GetOrAdd(typeof(Demo.Domain.IFoo), _ =>
                         {
                             var dependency1 = new Func<Demo.Domain.IBar>(((IServiceFactory<Demo.Domain.IBar>)this).CreateOrGetService);
                             var service = new Demo.Domain.Foo(dependency1);
                             return service;
                         });
                    return service;
                 }"));
        }
    }
}
