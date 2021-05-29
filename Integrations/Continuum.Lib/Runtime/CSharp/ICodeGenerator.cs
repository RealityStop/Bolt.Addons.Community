namespace Bolt.Addons.Integrations.Continuum.CSharp
{
    public interface ICodeGenerator
    {
        string Generate(int indent);
        string GenerateClean(int indent);
    }
}