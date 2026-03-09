namespace Unity.VisualScripting.Community.CSharp
{
    public interface IAssetCompiler
    {
        void Compile(UnityEngine.Object asset, PathConfig paths);
    }
}
