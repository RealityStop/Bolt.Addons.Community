using System;

namespace Bolt.Addons.Community.Utility
{
    public interface IDelegate
    {
        object GetDelegate();
        Type GetDelegateType();
        TypeParam[] parameters { get; }
    }
}
