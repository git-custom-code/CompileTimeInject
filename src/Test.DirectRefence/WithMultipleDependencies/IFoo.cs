namespace Test.DirectReference.WithMultipleDependencies
{
    public interface IFoo
    {
        BySingleContract.IFoo TransientDependency { get; }

        AsSingleton.IFoo SingletonDependency { get; }
    }
}
