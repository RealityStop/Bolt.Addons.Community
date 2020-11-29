using Ludiq;

namespace Bolt.Addons.Community.Utility
{
    [IncludeInSettings(true)]
    public sealed class AOTFunc1 : AOTFunc<object, object>
    {
        public override TypeParam[] parameters => new TypeParam[] { new TypeParam() { name = "param 0", type = typeof(object) } };
    }
}
