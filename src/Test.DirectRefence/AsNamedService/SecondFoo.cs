namespace Test.DirectReference.AsNamedService
{
    using CustomCode.CompileTimeInject.Annotations;

    [Export(typeof(IFoo), ServiceId = "SecondFooId")]
    public class SecondFoo : IFoo
    {
        public string Id { get; } = "2";
    }
}
