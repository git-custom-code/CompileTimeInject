namespace Test.DirectReference.ByMultipleImplementationsWithDependencies
{
    using CustomCode.CompileTimeInject.Annotations;

    [Export]
    public class Foo : IFoo
    {
        public Foo(AsSingleton.IFoo dependency)
        {
            Dependencies[0] = dependency;
        }

        public object[] Dependencies { get; } = new object[1];
    }
}
