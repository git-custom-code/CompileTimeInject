namespace Test.DirectReference.WithSingleDependency
{
    using CustomCode.CompileTimeInject;

    [Export]
    public class Foo : IFoo
    {
        public Foo(BySingleContract.IFoo dependency)
        {
            Dependency = dependency;
        }

        public BySingleContract.IFoo Dependency { get; }
    }
}
