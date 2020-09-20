namespace CustomCode.CompileTimeInject.ContainerGenerator.Extensions
{
    using Microsoft.CodeAnalysis;
    using System.Collections.Immutable;

    /// <summary>
    /// Extension methods for the <see cref="Diagnostic"/> type.
    /// </summary>
    public static class DiagnosticExtensions
    {
        #region Logic

        /// <summary>
        /// Query if the <paramref name="diagnostics"/> collection contains one or more errors.
        /// </summary>
        /// <param name="diagnostics"> The extended <see cref="Diagnostic"/> collection. </param>
        /// <returns> True if the collection contains one or more erros, false otherwise. </returns>
        public static bool HasErrors(this ImmutableArray<Diagnostic> diagnostics)
        {
            foreach(var diagnostic in diagnostics)
            {
                if (diagnostic.Severity == DiagnosticSeverity.Error)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Query if the <paramref name="diagnostics"/> collection contains one or more warnings.
        /// </summary>
        /// <param name="diagnostics"> The extended <see cref="Diagnostic"/> collection. </param>
        /// <returns> True if the collection contains one or more warnings, false otherwise. </returns>
        public static bool HasWarnings(this ImmutableArray<Diagnostic> diagnostics)
        {
            foreach (var diagnostic in diagnostics)
            {
                if (diagnostic.Severity == DiagnosticSeverity.Warning)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
