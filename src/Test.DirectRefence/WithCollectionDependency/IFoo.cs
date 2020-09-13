namespace Test.DirectReference.WithCollectionDependency
{
    using System.Collections.Generic;

    public interface IFoo
    {
        IEnumerable<ByMultipleImplementations.IFoo> Dependencies { get; }
    }
}
