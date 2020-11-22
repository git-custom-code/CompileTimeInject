namespace CustomCode.CompileTimeInject.ContainerGenerator.CodeGeneration
{
    using Annotations;
    using Metadata;
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
        public static string CommaSeparated(this IEnumerable<DependencyDescriptor> dependencies)
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
        /// <param name="dependency"> The extended <see cref="DependencyDescriptor"/>. </param>
        /// <returns> The contract of the injected dependency (i.e. the factories return type). </returns>
        public static string Contract(this DependencyDescriptor dependency)
        {
            if (dependency.Contract.FullName.StartsWith("System.Func<", StringComparison.Ordinal))
            {
                var start = "System.Func<".Length;
                var length = dependency.Contract.FullName.Length - start - 1;
                return dependency.Contract.FullName.Substring(start, length);
            }
            else if (dependency.Contract.FullName.StartsWith("Func<", StringComparison.Ordinal))
            {
                var start = "Func<".Length;
                var length = dependency.Contract.FullName.Length - start - 1;
                return dependency.Contract.FullName.Substring(start, length);
            }
            return dependency.Contract.FullName;
        }

        /// <summary>
        /// Get the correct parameters for the "ServiceCache.GetOrAdd" method call.
        /// </summary>
        /// <param name="service"> The <see cref="ServiceDescriptor"/> whose parameters should be retrieved. </param>
        /// <returns> The correct parameters for the "ServiceCache.GetOrAdd" method call. </returns>
        public static string CacheParameter(this ServiceDescriptor service)
        {
            if (string.IsNullOrEmpty(service.ServiceId))
            {
                return $"typeof({service.Contract.FullName}), _";
            }

            return $"typeof({service.Contract.FullName}), \"{service.ServiceId}\", _";
        }

        /// <summary>
        /// Get the correct "CreateOrGetService" / "CreateOrGetNamedService" call for the given <paramref name="dependency"/>.
        /// </summary>
        /// <param name="dependency"> The extended <see cref="DependencyDescriptor"/>. </param>
        /// <returns>
        /// The corret call for creating an instance of the given <paramref name="dependency"/>.
        /// </returns>
        public static string CreateOrGetService(this DependencyDescriptor dependency)
        {
            if (dependency.Contract.FullName.StartsWith("Func<", StringComparison.Ordinal) ||
                dependency.Contract.FullName.StartsWith("System.Func<", StringComparison.Ordinal))
            {
                if (string.IsNullOrEmpty(dependency.ServiceId))
                {
                    return $"new Func<{dependency.Contract()}>(((IServiceFactory<{dependency.Contract()}>)this).CreateOrGetService)";
                }
                return $"new Func<{dependency.Contract()}>(((INamedServiceFactory<{dependency.Contract()}>)this).CreateOrGetNamedService(\"{dependency.ServiceId}\"))";
            }

            if (string.IsNullOrEmpty(dependency.ServiceId))
            {
                return $"((IServiceFactory<{dependency.Contract.FullName}>)this).CreateOrGetService()";
            }
            return $"((INamedServiceFactory<{dependency.Contract.FullName}>)this).CreateOrGetNamedService(\"{dependency.ServiceId}\")";
        }

        /// <summary>
        /// Gets all <see cref="Lifetime.Scoped"/> services that implement the <paramref name="sharedContract"/>.
        /// </summary>
        /// <param name="sharedContract"> The extended <see cref="IGrouping{TKey, TElement}"/>. </param>
        /// <returns> All <see cref="Lifetime.Scoped"/> services that implement the <paramref name="sharedContract"/>. </returns>
        public static IEnumerable<ServiceDescriptor> ScopedServices(this IGrouping<TypeDescriptor, ServiceDescriptor> sharedContract)
        {
            return sharedContract.Where(service => service.Lifetime == Lifetime.Scoped);
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
