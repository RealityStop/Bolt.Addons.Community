using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEditor;

namespace Unity.VisualScripting.Community
{
    [InitializeOnLoad]
    public static class ProviderPatcher
    {
        static ProviderPatcher()
        {
            patchedProviders = new HashSet<object>();
            EnsurePatched();
        }

        public static void EnsurePatched()
        {
            PatchConstructorStubWriter();
        }

        private static readonly HashSet<object> patchedProviders = new HashSet<object>();

        /// <summary>
        /// Patch a provider instance to replace the behavior for a specific target type.
        /// </summary>
        public static void PatchProvider<TDecorated, TDecorator, TAttribute, TPatcherType>(SingleDecoratorProvider<TDecorated, TDecorator, TAttribute> providerInstance,
        Type targetType, Func<TDecorated, TDecorator> patchInstance, ref Action<TDecorated> triggerInstancePatch)
        where TAttribute : Attribute, IDecoratorAttribute
        {
            if (providerInstance == null) throw new ArgumentNullException(nameof(providerInstance));
            if (targetType == null) throw new ArgumentNullException(nameof(targetType));
            if (patchInstance == null) throw new ArgumentNullException(nameof(patchInstance));

            if (!patchedProviders.Add(providerInstance))
                return;

            var providerType = providerInstance.GetType();

            var definedField = providerType.GetField("definedDecoratorTypes", BindingFlags.NonPublic | BindingFlags.Instance);
            var resolvedField = providerType.GetField("resolvedDecoratorTypes", BindingFlags.NonPublic | BindingFlags.Instance);

            if (definedField != null && resolvedField != null)
            {
                var defined = (Dictionary<Type, Type>)definedField.GetValue(providerInstance);
                var resolved = (Dictionary<Type, Type>)resolvedField.GetValue(providerInstance);

                defined[targetType] = typeof(TPatcherType);
                resolved[targetType] = typeof(TPatcherType);
            }

            var decoratorsField = providerType.GetField("decorators", BindingFlags.NonPublic | BindingFlags.Instance);
            var decoratedsField = providerType.GetField("decorateds", BindingFlags.NonPublic | BindingFlags.Instance);

            if (decoratorsField != null && decoratedsField != null)
            {
                triggerInstancePatch += (v) =>
                {
                    var decorators = (Dictionary<TDecorated, TDecorator>)decoratorsField.GetValue(providerInstance);
                    var decorateds = (Dictionary<TDecorator, TDecorated>)decoratedsField.GetValue(providerInstance);

                    if (!decorators.ContainsKey(v))
                    {
                        var decorator = patchInstance(v);
                        if (decorator != null)
                        {
                            decorators[v] = decorator;
                            decorateds[decorator] = v;
                        }
                    }
                };
            }
        }

        private static void PatchConstructorStubWriter()
        {
            var provider = AotStubWriterProvider.instance;

            PatchProvider<object, AotStubWriter, AotStubWriterAttribute, ArrayConstructorInfoStubWriter>(provider,
            typeof(ConstructorInfo), (item) =>
            {
                if (item is ConstructorInfo c)
                {
                    return new ArrayConstructorInfoStubWriter(c);
                }
                return null;
            }, ref CreateArray.updateStubwriterCall);
        }
    }
}