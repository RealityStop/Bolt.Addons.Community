namespace Unity.VisualScripting.Community.CSharp
{
    public interface ICreateGenerationData
    {
        ControlGenerationData data { get; }
        public ControlGenerationData GetGenerationData(bool newIfDisposed = true);
        public ControlGenerationData CreateGenerationData();
    }
}
