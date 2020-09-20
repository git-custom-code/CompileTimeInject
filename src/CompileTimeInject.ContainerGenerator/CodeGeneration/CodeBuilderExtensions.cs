namespace CustomCode.CompileTimeInject.ContainerGenerator.CodeGeneration
{
    using Annotations;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A collection of various extension methods that can be used to improve readability when
    /// generation source code with a <see cref="CodeBuilder"/>.
    /// </summary>
    public static class CodeBuilderExtensions
    {
        #region Logic

        /// <summary>
        /// Format a collection of <paramref name="dependencies"/> as comma separated constructor
        /// parameters.
        /// </summary>
        /// <param name="dependencies"> The extended <see cref="IEnumerable{T}"/>. </param>
        /// <returns>
        /// A collection of <paramref name="dependencies"/> as comma separated constructor parameters.
        /// </returns>
        public static string CommaSeparated(this IEnumerable<TypeDescriptor> dependencies)
        {
            if (dependencies.Any())
            {
                var index = 1u;
                return string.Join(", ", dependencies.Select(d => $"dependency{index++}"));
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets the <paramref name="dependency"/>'s contract if it is injected as <see cref="Func{TResult}"/>.
        /// </summary>
        /// <param name="dependency"> The extended <see cref="TypeDescriptor"/>. </param>
        /// <returns> The contract of the injected dependency (i.e. the factories return type). </returns>
        public static string Contract(this TypeDescriptor dependency)
        {
            var start = "System.Func<".Length;
            var length = dependency.FullName.Length - start - 1;
            return dependency.FullName.Substring(start, length);
        }

        /// <summary>
        /// Query if the given <paramref name="dependency"/> is injected as factory of type <see cref="Func{TResult}"/>.
        /// </summary>
        /// <param name="dependency"> The extended <see cref="TypeDescriptor"/>. </param>
        /// <returns>
        /// True if the dependency is injected as factory of type <see cref="Func{TResult}"/>, false otherwise.
        /// </returns>
        public static bool IsFactory(this TypeDescriptor dependency)
        {
            return dependency.FullName.StartsWith("System.Func<");
        }

        /// <summary>
        /// Gets all <see cref="Lifetime.Singleton"/> services that implement the <paramref name="sharedContract"/>.
        /// </summary>
        /// <param name="sharedContract"> The extended <see cref="IGrouping{TKey, TElement}"/>. </param>
        /// <returns> All <see cref="Lifetime.Singleton"/> services that implement the <paramref name="sharedContract"/>. </returns>
        public static IEnumerable<ServiceDescriptor> SingletonServices(this IGrouping<TypeDescriptor, ServiceDescriptor> sharedContract)
        {
            return sharedContract.Where(service => service.Lifetime == Lifetime.Singleton);
        }

        /// <summary>
        /// Gets all <see cref="Lifetime.Transient"/> services that implement the <paramref name="sharedContract"/>.
        /// </summary>
        /// <param name="sharedContract"> The extended <see cref="IGrouping{TKey, TElement}"/>. </param>
        /// <returns> All <see cref="Lifetime.Transient"/> services that implement the <paramref name="sharedContract"/>. </returns>
        public static IEnumerable<ServiceDescriptor> TransientServices(this IGrouping<TypeDescriptor, ServiceDescriptor> sharedContract)
        {
            return sharedContract.Where(service => service.Lifetime == Lifetime.Transient);
        }

        #endregion
    }
}
