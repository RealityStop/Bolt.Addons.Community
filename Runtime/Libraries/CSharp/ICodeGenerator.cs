namespace Bolt.Addons.Libraries.CSharp
{
    public interface ICodeGenerator
    {
        string Generate(int indent);
        string GenerateClean(int indent);
    }
}