namespace Unity.VisualScripting.Community.CSharp
{
    public interface IRequireUpdateCode
    {
        void GenerateUpdateCode(ControlGenerationData data, CodeWriter writer);
    }
}