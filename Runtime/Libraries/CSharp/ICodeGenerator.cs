namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.ICodeGenerator")]
    public interface ICodeGenerator
    {
        string Generate(int indent);
        string GenerateClean(int indent);
    }
}