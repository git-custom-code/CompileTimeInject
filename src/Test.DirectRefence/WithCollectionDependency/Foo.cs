namespace Test.DirectReference.WithCollectionDependency
{
    using CustomCode.CompileTimeInject.Annotations;
    using System.Collections.Generic;

    [Export]
    public class Foo : IFoo
    {
        public Foo(IEnumerable<ByMultipleImplementations.IFoo> dependencies)
        {
            Dependencies = dependencies;
        }

        public IEnumerable<ByMultipleImplementations.IFoo> Dependencies { get; }
    }
}
