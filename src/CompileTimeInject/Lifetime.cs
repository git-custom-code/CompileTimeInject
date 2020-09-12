namespace CustomCode.CompileTimeInject
{
    /// <summary>
    /// Used in combination with an <see cref="ExportAttribute"/> to define the lifetime policy of a service.
    /// </summary>
    public enum Lifetime : byte
    {
        /// <summary> A new service instance is created per request. </summary>
        Transient = 0,
        /// <summary> A new service instance is created once per container. </summary>
        Singleton = 1
    }
}