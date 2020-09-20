namespace Test.DirectReference.WithFactoryDependency
{
    using CustomCode.CompileTimeInject.Annotations;
    using System;

    [Export]
    public class Foo : IFoo
    {
        public Foo(Func<BySingleContract.IFoo> dependencyFactory)
        {
            Dependency = dependencyFactory();
        }

        public BySingleContract.IFoo Dependency { get; }
    }
}
