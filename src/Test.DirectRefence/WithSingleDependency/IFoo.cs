namespace Test.DirectReference.WithSingleDependency
{
    public interface IFoo
    {
        BySingleContract.IFoo Dependency { get; }
    }
}
