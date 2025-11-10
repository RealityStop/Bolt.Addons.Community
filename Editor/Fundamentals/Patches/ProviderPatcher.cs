using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;
using System.Linq;
using Unity.VisualScripting.InputSystem;
namespace Unity.VisualScripting.Community
{
    [InitializeOnLoad]
    public static class ProviderPatcher
    {
        static ProviderPatcher()
        {
            EnsurePatched();
#if ENABLE_VERTICAL_FLOW
            PatchWidgets();
#endif
            // PatchGraphContext();
#if NEW_VARIABLES_UI
            PatchVariablesDeclarationsInspector();
#endif
        }

        public static void EnsurePatched()
        {
            PatchConstructorStubWriter();
        }

        private static void PatchWidgets()
        {
            IGraphContext current = null;
            var provider = DescriptorProvider.instance;
            PatchGlobalProvider(provider, typeof(IEventUnit), typeof(EventUnitDescriptor<>));
            GraphWindow.activeContextChanged += context =>
            {
                PatchUnitWidgets(context);
                PatchUnitPortWidgets(context);
                PatchUnitConnectionWidgets(context);
            };

            // This is just to make sure that the patch happens if for what ever reason 
            // activeContextChanged does not trigger
            if (current == null)
            {
                var context = GraphWindow.activeContext;
                PatchUnitWidgets(context);
                PatchUnitPortWidgets(context);
                PatchUnitConnectionWidgets(context);
            }
        }

        private static void PatchUnitWidgets(IGraphContext context)
        {
            if (context != null && context.graph is FlowGraph)
            {
                var provider = context.canvas.widgetProvider;
                // No need to instance patch the provider will construct the generic type.
                PatchGlobalProvider(provider, typeof(IUnit), typeof(UnitWidget<>));
                PatchGlobalProvider(provider, typeof(SubgraphUnit), typeof(SubgraphUnitWidget));
                PatchGlobalProvider(provider, typeof(StateUnit), typeof(StateUnitWidget));
                PatchGlobalProvider(provider, typeof(IEventUnit), typeof(EventUnitWidget));
                PatchGlobalProvider(provider, typeof(UnifiedVariableUnit), typeof(UnifiedVariableUnitWidget));
                PatchGlobalProvider(provider, typeof(Literal), typeof(LiteralWidget));
                PatchGlobalProvider(provider, typeof(MissingType), typeof(MissingTypeUnitWidget));
                PatchGlobalProvider(provider, typeof(GraphInput), typeof(GraphInputWidget));
                PatchGlobalProvider(provider, typeof(GraphOutput), typeof(GraphOutputWidget));
#if PACKAGE_INPUT_SYSTEM_EXISTS
                PatchGlobalProvider(provider, typeof(OnInputSystemEvent), typeof(InputSystemWidget));
#endif
            }
        }

        private static void PatchUnitPortWidgets(IGraphContext context)
        {
            if (context != null && context.graph is FlowGraph)
            {
                var provider = context.canvas.widgetProvider;
                // Ensure that new ports will use the new widget.
                // Needed for units that can change their amount of ports.
                PatchGlobalProvider<IGraphItem, IWidget, WidgetAttribute, ControlInputWidget>(provider, typeof(ControlInput));
                PatchGlobalProvider<IGraphItem, IWidget, WidgetAttribute, ControlOutputWidget>(provider, typeof(ControlOutput));
                PatchGlobalProvider<IGraphItem, IWidget, WidgetAttribute, ValueInputWidget>(provider, typeof(ValueInput));
                PatchGlobalProvider<IGraphItem, IWidget, WidgetAttribute, ValueOutputWidget>(provider, typeof(ValueOutput));
            }
        }

        private static void PatchUnitConnectionWidgets(IGraphContext context)
        {
            if (context != null && context.graph is FlowGraph)
            {
                var provider = context.canvas.widgetProvider;
                PatchGlobalProvider<IGraphItem, IWidget, WidgetAttribute, ControlConnectionWidget>(provider, typeof(ControlConnection));
                PatchGlobalProvider<IGraphItem, IWidget, WidgetAttribute, ValueConnectionWidget>(provider, typeof(ValueConnection));
            }
        }

        /// <summary>
        /// Patch a provider instance to replace the behavior for a specific instances of the target type.
        /// </summary>
        public static void PatchInstanceProvider<TDecorated, TDecorator, TAttribute, TPatcherType>(SingleDecoratorProvider<TDecorated, TDecorator, TAttribute> providerInstance,
        Type targetType, Func<TDecorated, TDecorator> patchInstance, ref Action<TDecorated> triggerInstancePatch, bool tryPatchGlobal = false)
        where TAttribute : Attribute, IDecoratorAttribute
        {
            if (providerInstance == null) throw new ArgumentNullException(nameof(providerInstance));
            if (targetType == null) throw new ArgumentNullException(nameof(targetType));
            if (patchInstance == null) throw new ArgumentNullException(nameof(patchInstance));

            var providerType = providerInstance.GetType();

            if (tryPatchGlobal)
            {
                var definedField = providerType.GetField("definedDecoratorTypes", BindingFlags.NonPublic | BindingFlags.Instance);
                var resolvedField = providerType.GetField("resolvedDecoratorTypes", BindingFlags.NonPublic | BindingFlags.Instance);

                if (definedField != null && resolvedField != null)
                {
                    var defined = (Dictionary<Type, Type>)definedField.GetValueOptimized(providerInstance);
                    var resolved = (Dictionary<Type, Type>)resolvedField.GetValueOptimized(providerInstance);

                    defined[targetType] = typeof(TPatcherType);
                    resolved[targetType] = typeof(TPatcherType);
                }
            }

            var decoratorsField = providerType.GetField("decorators", BindingFlags.NonPublic | BindingFlags.Instance);
            var decoratedsField = providerType.GetField("decorateds", BindingFlags.NonPublic | BindingFlags.Instance);

            if (decoratorsField != null && decoratedsField != null)
            {
                triggerInstancePatch += (v) =>
                {
                    var decorators = (Dictionary<TDecorated, TDecorator>)decoratorsField.GetValueOptimized(providerInstance);
                    var decorateds = (Dictionary<TDecorator, TDecorated>)decoratedsField.GetValueOptimized(providerInstance);

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

        /// <summary>
        /// Patch a provider instance to replace the behavior for a specific instances of the target type.
        /// </summary>
        public static void PatchInstanceProvider<TDecorated, TDecorator, TAttribute>(SingleDecoratorProvider<TDecorated, TDecorator, TAttribute> providerInstance,
        Type targetType, Func<TDecorated, TDecorator> patchInstance, ref Action<TDecorated> triggerInstancePatch, Type patchedType, bool tryPatchGlobal = false)
        where TAttribute : Attribute, IDecoratorAttribute
        {
            if (providerInstance == null) throw new ArgumentNullException(nameof(providerInstance));
            if (targetType == null) throw new ArgumentNullException(nameof(targetType));
            if (patchInstance == null) throw new ArgumentNullException(nameof(patchInstance));

            var providerType = providerInstance.GetType();

            if (tryPatchGlobal)
            {
                var definedField = providerType.GetField("definedDecoratorTypes", BindingFlags.NonPublic | BindingFlags.Instance);
                var resolvedField = providerType.GetField("resolvedDecoratorTypes", BindingFlags.NonPublic | BindingFlags.Instance);

                if (definedField != null && resolvedField != null)
                {
                    var defined = (Dictionary<Type, Type>)definedField.GetValueOptimized(providerInstance);
                    var resolved = (Dictionary<Type, Type>)resolvedField.GetValueOptimized(providerInstance);

                    defined[targetType] = patchedType;
                    resolved[targetType] = patchedType;
                }
            }

            var decoratorsField = providerType.GetField("decorators", BindingFlags.NonPublic | BindingFlags.Instance);
            var decoratedsField = providerType.GetField("decorateds", BindingFlags.NonPublic | BindingFlags.Instance);

            if (decoratorsField != null && decoratedsField != null)
            {
                triggerInstancePatch += (v) =>
                {
                    var decorators = (Dictionary<TDecorated, TDecorator>)decoratorsField.GetValueOptimized(providerInstance);
                    var decorateds = (Dictionary<TDecorator, TDecorated>)decoratedsField.GetValueOptimized(providerInstance);

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

        /// <summary>
        /// Patch a provider instance to replace the behavior for a specific instances of the target type.
        /// </summary>
        public static void PatchInstanceProvider<TDecorated, TDecorator, TAttribute>(SingleDecoratorProvider<TDecorated, TDecorator, TAttribute> providerInstance,
        Func<TDecorated, TDecorator> patchInstance, ref Action<TDecorated> triggerInstancePatch)
        where TAttribute : Attribute, IDecoratorAttribute
        {
            if (providerInstance == null) throw new ArgumentNullException(nameof(providerInstance));
            if (patchInstance == null) throw new ArgumentNullException(nameof(patchInstance));

            var providerType = providerInstance.GetType();

            var decoratorsField = providerType.GetField("decorators", BindingFlags.NonPublic | BindingFlags.Instance);
            var decoratedsField = providerType.GetField("decorateds", BindingFlags.NonPublic | BindingFlags.Instance);

            if (decoratorsField != null && decoratedsField != null)
            {
                triggerInstancePatch += (v) =>
                {
                    var decorators = (Dictionary<TDecorated, TDecorator>)decoratorsField.GetValueOptimized(providerInstance);
                    var decorateds = (Dictionary<TDecorator, TDecorated>)decoratedsField.GetValueOptimized(providerInstance);

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

        public static void PatchGlobalProvider<TDecorated, TDecorator, TAttribute, TPatcherType>(SingleDecoratorProvider<TDecorated, TDecorator, TAttribute> providerInstance,
        Type targetType)
        where TAttribute : Attribute, IDecoratorAttribute
        {
            if (providerInstance == null) throw new ArgumentNullException(nameof(providerInstance));
            if (targetType == null) throw new ArgumentNullException(nameof(targetType));

            var providerType = providerInstance.GetType();

            var definedField = providerType.GetField("definedDecoratorTypes", BindingFlags.NonPublic | BindingFlags.Instance);
            var resolvedField = providerType.GetField("resolvedDecoratorTypes", BindingFlags.NonPublic | BindingFlags.Instance);

            if (definedField != null && resolvedField != null)
            {
                var defined = (Dictionary<Type, Type>)definedField.GetValueOptimized(providerInstance);
                var resolved = (Dictionary<Type, Type>)resolvedField.GetValueOptimized(providerInstance);

                defined[targetType] = typeof(TPatcherType);
                resolved[targetType] = typeof(TPatcherType);
            }
        }

        public static void PatchGlobalProvider<TDecorated, TDecorator, TAttribute>(SingleDecoratorProvider<TDecorated, TDecorator, TAttribute> providerInstance,
        Type targetType, Type patcherType)
        where TAttribute : Attribute, IDecoratorAttribute
        {
            if (providerInstance == null) throw new ArgumentNullException(nameof(providerInstance));
            if (targetType == null) throw new ArgumentNullException(nameof(targetType));

            var providerType = providerInstance.GetType();

            var definedField = providerType.GetField("definedDecoratorTypes", BindingFlags.NonPublic | BindingFlags.Instance);
            var resolvedField = providerType.GetField("resolvedDecoratorTypes", BindingFlags.NonPublic | BindingFlags.Instance);

            if (definedField != null && resolvedField != null)
            {
                var defined = (Dictionary<Type, Type>)definedField.GetValueOptimized(providerInstance);
                var resolved = (Dictionary<Type, Type>)resolvedField.GetValueOptimized(providerInstance);

                defined[targetType] = patcherType;
                resolved[targetType] = patcherType;
            }
        }

        private static void PatchConstructorStubWriter()
        {
            var provider = AotStubWriterProvider.instance;

            PatchInstanceProvider<object, AotStubWriter, AotStubWriterAttribute, ArrayConstructorInfoStubWriter>(provider,
            typeof(ConstructorInfo), (item) =>
            {
                if (item is ConstructorInfo c)
                {
                    return new ArrayConstructorInfoStubWriter(c);
                }
                return null;
            }, ref CreateArray.updateStubwriterCall, true);
        }

        // I might use this later to patch the graph context to move the Utilities panel to the sidebars in the graph
        // private static void PatchGraphContext()
        // {
        //     var provider = GraphContextProvider.instance;

        //     PatchGlobalProvider<GraphReference, IGraphContext, GraphContextAttribute, PatchedGraphContext>(provider,
        //     typeof(FlowGraph));

        //     var editorProvider = EditorProvider.instance;

        //     PatchGlobalProvider<Metadata, Inspector, EditorAttribute, PatchedUnitEditor>(editorProvider,
        //     typeof(IUnit));

        //     PatchGlobalProvider<Metadata, Inspector, EditorAttribute, PatchedNesterUnitEditor>(editorProvider,
        //     typeof(INesterUnit));

        //     PatchGlobalProvider<Metadata, Inspector, EditorAttribute, PatchedSuperUnitEditor>(editorProvider,
        //     typeof(SubgraphUnit));

        //     PatchGlobalProvider<Metadata, Inspector, EditorAttribute, PatchedStateUnitEditor>(editorProvider,
        //     typeof(StateUnit));
        // }

        private static void PatchVariablesDeclarationsInspector()
        {
            var provider = InspectorProvider.instance;

            PatchGlobalProvider<Metadata, Inspector, InspectorAttribute, PatchedVariableDeclarationsInspector>(provider,
            typeof(VariableDeclarations));
        }
    }
}