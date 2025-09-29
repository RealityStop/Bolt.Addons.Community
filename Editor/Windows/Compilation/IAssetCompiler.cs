namespace Unity.VisualScripting.Community
{
    public interface IAssetCompiler
    {
        void Compile(UnityEngine.Object asset, PathConfig paths);
    }
}
