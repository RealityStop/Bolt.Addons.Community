using Ludiq;

namespace Bolt.Addons.Community.Fundamentals.Units.Collections
{
    /// <summary>
    /// An enum of Linq query operations.
    /// </summary>
    [RenamedFrom("Lasm.BoltExtensions.QueryOperation")]
    [RenamedFrom("Lasm.UAlive.QueryOperation")]
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