using System;
using Unity.VisualScripting.Community.CSharp;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    /// <summary>
    /// A generator that sets up a body { } for you. Easily add what comes before, during, and after the body.
    /// </summary>
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.BodyGenerator")]
    public abstract class BodyGenerator : ConstructGenerator
    {
        public bool hideBrackets;
        IDisposable nodeScope = null;
        /// <summary>
        /// Generates the entire body, includes before and after, as a string.
        /// </summary>
        public override void Generate(CodeWriter writer, ControlGenerationData data)
        {
            Initialize(writer);
            if (owner != null)
                nodeScope = writer.BeginNode(owner);
            // write a empty string so if Generate before does not Write anything LastGeneratedCode will be empty
            writer.Write("");

            GenerateBefore(writer, data);

            if (!hideBrackets)
            {
                writer.WriteLine("{");
                writer.Indent();
            }

            GenerateBody(writer, data);

            if (!hideBrackets)
            {
                writer.Unindent();
                writer.WriteLine("}");
            }

            writer.Write("");

            GenerateAfter(writer, data);

            if (!string.IsNullOrWhiteSpace(writer.LastGeneratedCode))
            {
                writer.NewLine();
            }
            nodeScope?.Dispose();
        }

        public virtual void Initialize(CodeWriter writer) { }

        /// <summary>
        /// Override to generate what comes before the body. Such as a method or type declaration, or even a lambda expression.
        /// </summary>
        protected abstract void GenerateBefore(CodeWriter writer, ControlGenerationData data);

        /// <summary>
        /// Override to generate what is inside the body.
        /// </summary>
        protected abstract void GenerateBody(CodeWriter writer, ControlGenerationData data);

        /// <summary>
        /// Override to generate what comes after the body. For instance, you may way to close a lambda expression by adding a ); after.
        /// </summary>
        protected abstract void GenerateAfter(CodeWriter writer, ControlGenerationData data);
    }
}