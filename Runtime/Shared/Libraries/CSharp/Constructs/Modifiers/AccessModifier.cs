namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    /// <summary>
    /// The scope of a C# construct. Excludes root constructs, such as a class declaration. Use RootAccessModifier for those.
    /// </summary>
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.AccessModifier")]
    public enum AccessModifier
    {
        Public = 0,
        Private = 6,
        Protected = 4,
        Internal = 3,
        ProtectedInternal = 2,
        PrivateProtected = 5,
        None = 1
    }
}