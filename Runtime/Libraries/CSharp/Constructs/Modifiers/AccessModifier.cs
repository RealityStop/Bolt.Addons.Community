namespace Unity.VisualScripting.Community.Libraries.CSharp
{ 
    /// <summary>
    /// The scope of a C# construct. Excludes root constructs, such as a class declaration. Use RootAccessModifier for those.
    /// </summary>
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.AccessModifier")]
    public enum AccessModifier
    {
        Public,
        Private,
        Protected,
        Internal,
        ProtectedInternal,
        PrivateProtected
    }
}