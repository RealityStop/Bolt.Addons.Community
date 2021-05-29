using System;

namespace Bolt.Addons.Community.Utility
{
    public interface IDelegate
    {
        string DisplayName { get; }
        object GetDelegate();
        Type GetDelegateType();
        TypeParam[] parameters { get; }
    }
}
