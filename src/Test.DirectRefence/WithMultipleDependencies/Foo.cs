namespace Test.DirectReference.WithMultipleDependencies
{
    using CustomCode.CompileTimeInject;

    [Export]
    public class Foo : IFoo
    {
        public Foo(BySingleContract.IFoo transientDependency, AsSingleton.IFoo singletonDependency)
        {
            TransientDependency = transientDependency;
            SingletonDependency = singletonDependency;
        }

        public BySingleContract.IFoo TransientDependency { get; }

        public AsSingleton.IFoo SingletonDependency { get; }
    }
}
