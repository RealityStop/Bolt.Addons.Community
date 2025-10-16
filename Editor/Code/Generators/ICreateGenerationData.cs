namespace Unity.VisualScripting.Community 
{
    public interface ICreateGenerationData
    {
        ControlGenerationData data { get; }
        public ControlGenerationData GetGenerationData(bool newIfDisposed = true);
        public ControlGenerationData CreateGenerationData();
    } 
}
