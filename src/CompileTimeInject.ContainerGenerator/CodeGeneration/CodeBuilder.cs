namespace CustomCode.CompileTimeInject.ContainerGenerator.CodeGeneration
{
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
        /// Begin a new code scope (i.e. "{") and add the given <paramref name="linesOfCode"/>
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
        /// End the current code scope (i.e. "}") and add the given <paramref name="linesOfCode"/>
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
