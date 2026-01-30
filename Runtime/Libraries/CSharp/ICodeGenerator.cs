using Unity.VisualScripting.Community.CSharp;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.ICodeGenerator")]
    public interface ICodeGenerator
    {
        void Generate(CodeWriter writer, ControlGenerationData data);
        string GenerateClean(CodeWriter writer, ControlGenerationData data);
    }
}
