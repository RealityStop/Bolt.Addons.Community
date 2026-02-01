namespace Unity.VisualScripting.Community.CSharp
{
    public interface IRequireAwakeCode
    {
        void GenerateAwakeCode(ControlGenerationData data, CodeWriter writer);
    }
}