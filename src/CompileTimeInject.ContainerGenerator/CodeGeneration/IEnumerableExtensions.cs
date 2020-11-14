namespace CustomCode.CompileTimeInject.ContainerGenerator.CodeGeneration
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A collection of various extension methods for the <see cref="IEnumerable{T}"/> type
    /// that can be used to improve readability.
    /// </summary>
    public static class IEnumerableExtensions
    {
        #region Logic

        /// <summary>
        /// Check if the extended <paramref name="collection"/> is empty.
        /// </summary>
        /// <typeparam name="T"> The <see cref="System.Type"/> of objects to enumerate. </typeparam>
        /// <param name="collection"> The extended <see cref="IEnumerable{T}"/> instance. </param>
        /// <returns> True if the extended collection is empty, false otherwise. </returns>
        public static bool None<T>(this IEnumerable<T> collection)
        {
            return collection.Any() == false;
        }

        #endregion
    }
}
