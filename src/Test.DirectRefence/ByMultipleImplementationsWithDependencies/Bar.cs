namespace Test.DirectReference.ByMultipleImplementationsWithDependencies
{
    using CustomCode.CompileTimeInject;
    using Test.DirectReference.ByGenericContract;

    [Export]
    public class Bar : IFoo
    {
        public Bar(IFoo<int[,]> genericDependency, BySingleContract.IFoo dependency)
        {
            Dependencies[0] = genericDependency;
            Dependencies[1] = dependency;
        }

        public object[] Dependencies { get; } = new object[2];
    }
}
