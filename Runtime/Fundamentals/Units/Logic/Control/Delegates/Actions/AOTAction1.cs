using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility
{
    [IncludeInSettings(true)]
    public sealed class AOTAction1 : AOTAction<object>
    {
        public override TypeParam[] parameters => new TypeParam[] { new TypeParam() { name = "param 0", type = typeof(object) } };
    }
}
