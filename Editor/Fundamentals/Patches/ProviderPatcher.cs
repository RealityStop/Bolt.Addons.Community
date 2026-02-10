using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
#if PACKAGE_INPUT_SYSTEM_EXISTS
using Unity.VisualScripting.InputSystem;
using UnityEngine;
using System.Collections;


#endif

#if VISUAL_SCRIPTING_1_7
using SUnit = Unity.VisualScripting.SubgraphUnit;
#else
using SUnit = Unity.VisualScripting.SuperUnit;
#endif

namespace Unity.VisualScripting.Community
{
    [InitializeOnLoad]
    public static class ProviderPatcher
    {
        internal static bool isPatched;
        static ProviderPatcher()
        {
            EnsurePatched();
            PatchVariablesDeclarationsInspector();
            PatchWidgets();
            // PatchGraphContext();
        }

        public static void EnsurePatched()
        {
            PatchConstructorStubWriter();
        }

        private static FieldInfo ReferenceDataField = typeof(GraphWindow).GetField("referenceData", BindingFlags.Instance | BindingFlags.NonPublic);

        private static void PatchWidgets()
        {
            var descriptorProvider = DescriptorProvider.instance;
#if ENABLE_VERTICAL_FLOW
            PatchGlobalProvider(descriptorProvider, typeof(IEventUnit), typeof(EventUnitDescriptor<>));
#endif
            GraphWindow.activeContextChanged += context =>
            {
                if (context != null && context.graph is FlowGraph)
                {
                    var provider = context.canvas.widgetProvider;
                    PatchGlobalProvider(provider, typeof(UnifiedVariableUnit), typeof(UnifiedVariableUnitWidget));
                }
#if NEW_UNIT_UI
                PatchUnitWidgets(context);
#endif
                PatchUnitPortWidgets(context);
                PatchUnitConnectionWidgets(context);
            };

            bool set = false;
            // Trigger the patch for each of the windows
            // this should ensure that it patches the providers before any call to
            // canvas.Widget can be made.
            foreach (var tab in Resources.FindObjectsOfTypeAll<GraphWindow>())
            {
                tab.reference = (ReferenceDataField.GetValue(tab) as GraphPointerData).ToReference(false);

                var context = tab.context;

                isPatched = set = context != null && context.graph is FlowGraph;

                if (context != null && context.graph is FlowGraph)
                {
                    var provider = context.canvas.widgetProvider;
                    PatchGlobalProvider(provider, typeof(UnifiedVariableUnit), typeof(UnifiedVariableUnitWidget));
                }
#if NEW_UNIT_UI
                PatchUnitWidgets(context);
#endif
                PatchUnitPortWidgets(context);
                PatchUnitConnectionWidgets(context);
                if (!string.IsNullOrEmpty(context?.windowTitle))
                {
                    EditorApplication.delayCall += () =>
                    {
                        tab.titleContent = new GUIContent(context.windowTitle, BoltCore.Icons.window?[IconSize.Small]);
                    };
                }
            }

            if (!set)
            {
                EditorApplicationUtility.onHierarchyChange += TryPatchAllWindows;
            }
        }

        private static readonly HashSet<int> PatchedWindows = new HashSet<int>();

        private static void TryPatchAllWindows()
        {
            foreach (var tab in Resources.FindObjectsOfTypeAll<GraphWindow>())
            {
                TryPatchWindow(tab);
            }

            EditorApplicationUtility.onHierarchyChange -= TryPatchAllWindows;
        }

        private static void TryPatchWindow(GraphWindow tab)
        {
            if (tab == null) return;

            var id = tab.GetInstanceID();
            if (PatchedWindows.Contains(id)) return;

            if (tab.reference == null)
                tab.reference = (ReferenceDataField.GetValue(tab) as GraphPointerData).ToReference(false);

            tab.Validate();
            tab.MatchSelection();

            var context = tab.context;
            if (context == null || !(context.graph is FlowGraph graph)) return;
            if (context.canvas == null) return;

            var provider = context.canvas.widgetProvider;
            if (provider == null) return;

            isPatched = true;

            PatchGlobalProvider(provider, typeof(UnifiedVariableUnit), typeof(UnifiedVariableUnitWidget));

#if NEW_UNIT_UI
            PatchUnitWidgets(context);
#endif

            PatchUnitPortWidgets(context);
            PatchUnitConnectionWidgets(context);
            context.canvas.CacheWidgetCollections();

            if (!string.IsNullOrEmpty(context?.windowTitle))
            {
                EditorApplication.delayCall += () =>
                {
                    if (tab != null)
                        tab.titleContent = new GUIContent(
                            context.windowTitle,
                            BoltCore.Icons.window?[IconSize.Small]
                        );
                };
            }

            PatchedWindows.Add(id);
        }

        private static void PatchUnitWidgets(IGraphContext context)
        {
            if (context != null && context.graph is FlowGraph graph)
            {
                var provider = context.canvas.widgetProvider;
                PatchGlobalProvider(provider, typeof(IUnit), typeof(UnitWidget<>));
                PatchGlobalProvider(provider, typeof(SUnit), typeof(SubgraphUnitWidget));
                PatchGlobalProvider(provider, typeof(StateUnit), typeof(StateUnitWidget));
                PatchGlobalProvider(provider, typeof(IEventUnit), typeof(EventUnitWidget));
                PatchGlobalProvider(provider, typeof(Literal), typeof(LiteralWidget));
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
                PatchGlobalProvider(provider, typeof(MissingType), typeof(MissingTypeUnitWidget));
#endif
                PatchGlobalProvider(provider, typeof(GraphInput), typeof(GraphInputWidget));
                PatchGlobalProvider(provider, typeof(GraphOutput), typeof(GraphOutputWidget));
                PatchGlobalProvider(provider, typeof(TriggerStateTransition), typeof(TriggerStateTransitionWidget));
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
#if ENABLE_VERTICAL_FLOW
                PatchGlobalProvider<IGraphItem, IWidget, WidgetAttribute, ControlInputWidget>(provider, typeof(ControlInput));
                PatchGlobalProvider<IGraphItem, IWidget, WidgetAttribute, ControlOutputWidget>(provider, typeof(ControlOutput));
#endif
                PatchGlobalProvider<IGraphItem, IWidget, WidgetAttribute, ValueInputWidget>(provider, typeof(ValueInput));
                PatchGlobalProvider<IGraphItem, IWidget, WidgetAttribute, ValueOutputWidget>(provider, typeof(ValueOutput));
            }
        }

        private static void PatchUnitConnectionWidgets(IGraphContext context)
        {
            if (context != null && context.graph is FlowGraph)
            {
                var provider = context.canvas.widgetProvider;
#if ENABLE_VERTICAL_FLOW
                PatchGlobalProvider<IGraphItem, IWidget, WidgetAttribute, ControlConnectionWidget>(provider, typeof(ControlConnection));
#endif
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
#if NEW_VARIABLES_UI
            PatchGlobalProvider<Metadata, Inspector, InspectorAttribute, PatchedVariableDeclarationsInspector>(provider,
            typeof(VariableDeclarations));
#else
            PatchGlobalProvider<Metadata, Inspector, InspectorAttribute, PatchedVariableDeclarationInspector>(provider,
            typeof(VariableDeclaration));
#endif
        }
    }
}