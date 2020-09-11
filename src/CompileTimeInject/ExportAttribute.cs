namespace CustomCode.CompileTimeInject
{
    using System;

    /// <summary>
    /// Use this attribute on the class level to tell the source generator how to generate code
    /// for the annotated service type.
    /// </summary>
    /// <example>
    ///
    /// - If the type implements no interface it is registered by the type itself:
    /// <![CDATA[
    /// [Export]
    /// public sealed class Foo
    /// { }
    /// ]]>
    /// - If the type implements a single interface it is registered by the interface:
    /// <![CDATA[
    /// [Export]
    /// public sealed class Foo : IFoo
    /// { }
    /// ]]>
    /// - If the type implements multiple interfaces register it once per interface:
    /// <![CDATA[
    /// [Export]
    /// public sealed class Foo : IFoo, IBar
    /// { }
    /// ]]>
    ///
    /// </example>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ExportAttribute : Attribute
    {
        #region Dependencies

        /// <summary>
        /// Creates a new instance of the <see cref="ExportAttribute"/> type.
        /// </summary>
        public ExportAttribute()
            : this(null, Lifetime.Transient)
        { }

        /// <summary>
        /// Creates a new instance of the <see cref="ExportAttribute"/> type.
        /// </summary>
        /// <param name="serviceContract">
        /// An optional contract type (e.g. an implemented interface) for created service instances.
        /// </param>
        public ExportAttribute(Type? serviceContract)
            : this(serviceContract, Lifetime.Transient)
        { }

        /// <summary>
        /// Creates a new instance of the <see cref="ExportAttribute"/> type.
        /// </summary>
        /// <param name="lifetime"> The lifetime policy for created service instances. </param>
        public ExportAttribute(Lifetime lifetime)
            : this(null, lifetime)
        { }

        /// <summary>
        /// Creates a new instance of the <see cref="ExportAttribute"/> type.
        /// </summary>
        /// <param name="serviceContract">
        /// An optional contract type (e.g. an implemented interface) for created service instances.
        /// </param>
        /// <param name="lifetime"> The lifetime policy for created service instances. </param>
        public ExportAttribute(Type? serviceContract = null, Lifetime lifetime = Lifetime.Transient)
        {
            ServiceContract = serviceContract;
            Lifetime = lifetime;
        }

        #endregion

        #region Data

        /// <summary>
        /// Gets an optional contract type (e.g. an implemented interface) for created service instances.
        /// </summary>
        public Type? ServiceContract { get; }

        /// <summary>
        /// Gets the lifetime policy for created service instances.
        /// </summary>
        public Lifetime Lifetime { get; }

        #endregion
    }
}