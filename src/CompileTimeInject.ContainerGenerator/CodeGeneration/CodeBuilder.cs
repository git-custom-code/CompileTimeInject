namespace CustomCode.CompileTimeInject.ContainerGenerator.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// A specialized <see cref="StringBuilder"/> implementation that can be used to dynamically
    /// create c# source code.
    /// </summary>
    public sealed class CodeBuilder
    {
        #region Dependencies

        /// <summary>
        /// Creates a new instance of the <see cref="CodeBuilder"/> type.
        /// </summary>
        /// <param name="linesOfCode">
        /// The initial line(s) of source code (without indent) to be added to the builder.
        /// Usually this should be a single line of code with the namespace declaration.
        /// </param>
        public CodeBuilder(params string[] linesOfCode)
        {
            AppendLines(linesOfCode);
        }

        #endregion

        #region Data

        /// <summary>
        /// The current indent (that depends on the number of currently open code scopes) for a new line of code.
        /// </summary>
        private string Indent { get; set; } = string.Empty;

        /// <summary>
        /// The number of currently open code scopes (that define the indent for a new line of code).
        /// </summary>
        private uint OpenScopeCount { get; set; } = 0;

        /// <summary>
        /// The internal <see cref="StringBuilder"/> that is used to build the source code.
        /// </summary>
        private StringBuilder SourceCode { get; } = new StringBuilder();

        #endregion

        #region Logic

        /// <summary>
        /// Begins a new code scope (i.e. "{") and add the given <paramref name="linesOfCode"/>
        /// to the builder using the scope's indent.
        /// </summary>
        /// <param name="linesOfCode"> The line(s) of source code to be added to the builder. </param>
        /// <returns> The current builder's instance in order to enable fluent style api syntax. </returns>
        public CodeBuilder BeginScope(params string[] linesOfCode)
        {
            SourceCode.AppendLine($"{Indent}{{");

            OpenScopeCount++;
            Indent = new string(' ', (int)(OpenScopeCount * 4));

            AppendLines(linesOfCode);
            return this;
        }

        /// <summary>
        /// Begins a new code scope for an inline anonymous lambda (i.e. "{") and add the given
        /// <paramref name="linesOfCode"/> as the lambda's body to the builder using an additional indent.
        /// </summary>
        /// <param name="linesOfCode"> The line(s) of source code to be added to the inline lambdas body. </param>
        /// <returns> The current builder's instance in order to enable fluent style api syntax. </returns>
        public CodeBuilder BeginInlineLambdaScope(params string[] linesOfCode)
        {
            OpenScopeCount++;
            Indent = new string(' ', (int)(OpenScopeCount * 4));

            SourceCode.AppendLine($"{Indent}{{");

            OpenScopeCount++;
            Indent = new string(' ', (int)(OpenScopeCount * 4));

            AppendLines(linesOfCode);
            return this;
        }

        /// <summary>
        /// Ends the current code scope (i.e. "}") and add the given <paramref name="linesOfCode"/>
        /// to the builder using the outer scope's indent.
        /// </summary>
        /// <param name="linesOfCode"> The line(s) of source code to be added to the builder. </param>
        /// <returns> The current builder's instance in order to enable fluent style api syntax. </returns>
        public CodeBuilder EndScope(params string[] linesOfCode)
        {
            if (OpenScopeCount == 1)
            {
                OpenScopeCount = 0;
                Indent = string.Empty;
            }
            else if (OpenScopeCount > 1)
            {
                OpenScopeCount--;
                Indent = new string(' ', (int)(OpenScopeCount * 4));
            }

            SourceCode.AppendLine($"{Indent}}}");
            AppendLines(linesOfCode);
            return this;
        }

        /// <summary>
        /// Ends the current inline anonymous lambda scope (i.e. "}") with an optional additional closure (e.g. ");").
        /// </summary>
        /// <param name="additionalClosure"> An additional optional closure (e.g. ");") to be appended to the scope. </param>
        /// <returns> The current builder's instance in order to enable fluent style api syntax. </returns>
        public CodeBuilder EndInlineLambdaScope(string? additionalClosure)
        {
            if (OpenScopeCount == 1)
            {
                OpenScopeCount = 0;
                Indent = string.Empty;
            }
            else if (OpenScopeCount > 1)
            {
                OpenScopeCount--;
                Indent = new string(' ', (int)(OpenScopeCount * 4));
            }

            if (string.IsNullOrEmpty(additionalClosure))
            {
                SourceCode.AppendLine($"{Indent}}}");
            }
            else
            {
                SourceCode.AppendLine($"{Indent}}}{additionalClosure}");
            }

            if (OpenScopeCount == 1)
            {
                OpenScopeCount = 0;
                Indent = string.Empty;
            }
            else if (OpenScopeCount > 1)
            {
                OpenScopeCount--;
                Indent = new string(' ', (int)(OpenScopeCount * 4));
            }

            return this;
        }

        /// <summary>
        /// Append a formatted line of code to the current builder for each item in the <paramref name="collection"/>
        /// using the current code scope's indent.
        /// </summary>
        /// <typeparam name="T"> The type of the <paramref name="collection"/>'s items. </typeparam>
        /// <param name="collection"> The source <see cref="IEnumerable{T}"/>. </param>
        /// <param name="formatLineOfCode">
        /// A delegate that takes each <paramref name="collection"/> item and returns a formatted line of code
        /// that is appended to the builder using the current code scope's indent.
        /// </param>
        /// <returns> The current builder's instance in order to enable fluent style api syntax. </returns>
        public CodeBuilder ForEach<T>(IEnumerable<T> collection, Func<T, string> formatLineOfCode)
        {
            foreach (var item in collection)
            {
                var lineOfCode = formatLineOfCode(item);
                SourceCode.AppendLine($"{Indent}{lineOfCode}");
            }
            return this;
        }

        /// <summary>
        /// Append a formatted line of code to the current builder for each item in the <paramref name="collection"/>
        /// using the current code scope's indent.
        /// </summary>
        /// <typeparam name="T"> The type of the <paramref name="collection"/>'s items. </typeparam>
        /// <param name="collection"> The source <see cref="IEnumerable{T}"/>. </param>
        /// <param name="formatLineOfCode">
        /// A delegate that takes each <paramref name="collection"/> item and the item's index in the collection
        /// and returns a formatted line of code that is appended to the builder using the current code scope's indent.
        /// </param>
        /// <returns> The current builder's instance in order to enable fluent style api syntax. </returns>
        public CodeBuilder ForEach<T>(IEnumerable<T> collection, Func<T, uint, string> formatLineOfCode)
        {
            var index = 0u;
            foreach (var item in collection)
            {
                ++index;
                var lineOfCode = formatLineOfCode(item, index);
                SourceCode.AppendLine($"{Indent}{lineOfCode}");
            }
            return this;
        }

        /// <summary>
        /// Execute nested code generation on the current builder for each item in the
        /// <paramref name="collection"/> using the current code scope's indent.
        /// </summary>
        /// <typeparam name="T"> The type of the <paramref name="collection"/>'s items. </typeparam>
        /// <param name="collection"> The source <see cref="IEnumerable{T}"/>. </param>
        /// <param name="nestedBuilderAction">
        /// A delegate that takes each <paramref name="collection"/> item and the <see cref="CodeBuilder"/>
        /// instance and allows execution of nested code generation for each item.
        /// </param>
        /// <returns> The current builder's instance in order to enable fluent style api syntax. </returns>
        public CodeBuilder ForEach<T>(IEnumerable<T> collection, Action<T, CodeBuilder> nestedBuilderAction)
        {
            foreach (var item in collection)
            {
                nestedBuilderAction(item, this);
            }
            return this;
        }

        /// <summary>
        /// Execute nested code generation on the current builder for each item in the
        /// <paramref name="collection"/> using the current code scope's indent.
        /// </summary>
        /// <typeparam name="T"> The type of the <paramref name="collection"/>'s items. </typeparam>
        /// <param name="collection"> The source <see cref="IEnumerable{T}"/>. </param>
        /// <param name="nestedBuilderAction">
        /// A delegate that takes each <paramref name="collection"/> item, the item's index in the collection
        /// and the <see cref="CodeBuilder"/> instance and allows execution of nested code generation for each item.
        /// </param>
        /// <returns> The current builder's instance in order to enable fluent style api syntax. </returns>
        public CodeBuilder ForEach<T>(IEnumerable<T> collection, Action<T, uint, CodeBuilder> nestedBuilderAction)
        {
            var index = 0u;
            foreach (var item in collection)
            {
                ++index;
                nestedBuilderAction(item, index, this);
            }
            return this;
        }

        /// <summary>
        /// Often used in combination with <see cref="ForEach{T}(IEnumerable{T}, Action{T, CodeBuilder})"/>
        /// this method allows to continue generation code for the nested <see cref="CodeBuilder"/>.
        /// </summary>
        /// <param name="linesOfCode"> The line(s) of source code to be added to the builder. </param>
        /// <returns> The current builder's instance in order to enable fluent style api syntax. </returns>
        public CodeBuilder ContinueWith(params string[] linesOfCode)
        {
            AppendLines(linesOfCode);
            return this;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return SourceCode.ToString();
        }

        /// <summary>
        /// Appends the specified lines of code to the internal <see cref="StringBuilder"/> using
        /// the current <see cref="Indent"/>.
        /// </summary>
        /// <param name="linesOfCode">
        /// The lines of code to be added to the internal <see cref="StringBuilder"/>.
        /// </param>
        private void AppendLines(string[] linesOfCode)
        {
            foreach (var lineOfCode in linesOfCode)
            {
                if (string.IsNullOrEmpty(lineOfCode))
                {
                    SourceCode.AppendLine();
                }
                else
                {
                    SourceCode.AppendLine($"{Indent}{lineOfCode}");
                }
            }
        }

        #endregion
    }
}
