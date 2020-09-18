namespace Test.DirectReference.WithFactoryDependency
{
    public interface IFoo
    {
        BySingleContract.IFoo Dependency { get; }
    }
}
