using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility
{
    [IncludeInSettings(true)]
    public sealed class AOTFunc1 : AOTFunc<object, object>
    {
        public override TypeParam[] parameters => new TypeParam[] { new TypeParam() { name = "param 0", type = typeof(object) } };

        public override string DisplayName => "AOT Func 1";
    }
}
