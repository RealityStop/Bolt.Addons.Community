namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// An enum of Linq query operations.
    /// </summary>
    [RenamedFrom("Lasm.BoltExtensions.QueryOperation")]
    [RenamedFrom("Lasm.UAlive.QueryOperation")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.Units.Collections.QueryOperation")]
    public enum QueryOperation
    {
        Any,
        AnyWithCondition,
        First,
        FirstOrDefault,
        OrderBy,
        OrderByDescending,
        Single,
        Where
    }
}