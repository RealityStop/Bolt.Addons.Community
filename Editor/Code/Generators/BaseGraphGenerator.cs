using System;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting.Community.Utility;

#if PACKAGE_INPUT_SYSTEM_EXISTS
using Unity.VisualScripting.InputSystem;
using UnityEngine.InputSystem;
#endif

namespace Unity.VisualScripting.Community.CSharp
{
    public abstract class BaseGraphGenerator<T> : CodeGenerator<T> where T : UnityEngine.Object
    {
        #region Static Fields
        protected static readonly Dictionary<string, string> EVENT_NAMES = new Dictionary<string, string>()
        {
            { "OnStart", "Start" },
            { "OnUpdate", "Update" },
            { "OnAwake", "Awake" },
            { "OnFixedUpdate", "FixedUpdate" },
            { "OnLateUpdate", "LateUpdate" },
        };

        protected static readonly HashSet<Type> UNITY_METHOD_TYPES = new HashSet<Type>
        {
#if MODULE_PHYSICS_EXISTS
            typeof(OnCollisionEnter),
            typeof(OnCollisionExit),
            typeof(OnCollisionStay),
            typeof(OnJointBreak),
            typeof(OnTriggerEnter),
            typeof(OnTriggerExit),
            typeof(OnTriggerStay),
            typeof(OnControllerColliderHit),
            typeof(OnParticleCollision),
#endif
#if MODULE_PHYSICS_2D_EXISTS
            typeof(OnCollisionEnter2D),
            typeof(OnCollisionExit2D),
            typeof(OnCollisionStay2D),
            typeof(OnJointBreak2D),
            typeof(OnTriggerEnter2D),
            typeof(OnTriggerExit2D),
            typeof(OnTriggerStay2D),
#endif
            typeof(OnApplicationFocus),
            typeof(OnApplicationPause),
            typeof(OnApplicationQuit),
            typeof(Start),
            typeof(Update),
            typeof(FixedUpdate),
            typeof(LateUpdate),
            typeof(OnBecameVisible),
            typeof(OnBecameInvisible),
#if PACKAGE_INPUT_SYSTEM_EXISTS
            typeof(OnInputSystemEventButton),
            typeof(OnInputSystemEventVector2),
            typeof(OnInputSystemEventFloat),
#endif
        };
        #endregion

        // Todo: find a better way to handle these
        protected Dictionary<CustomEvent, int> _customEventIds;
        protected Dictionary<BoltNamedAnimationEvent, int> _namedAnimationEventIds;
        protected Dictionary<BoltUnityEvent, int> _unityEventIds;

        protected readonly HashSet<Unit> _updateUnits = new HashSet<Unit>();
        protected List<Unit> _allUnits;
        protected List<Unit> _eventUnits;
        protected Dictionary<Type, int> _generatorCount = new Dictionary<Type, int>();

        // Todo: find a better way to handle these
        protected List<IEventUnit> focusTrueUnits = new List<IEventUnit>();
        protected List<IEventUnit> focusFalseUnits = new List<IEventUnit>();
        protected List<IEventUnit> pauseTrueUnits = new List<IEventUnit>();
        protected List<IEventUnit> pauseFalseUnits = new List<IEventUnit>();

        private Dictionary<string, (MethodGenerator method, bool createNew)> RequiredMethods = new Dictionary<string, (MethodGenerator, bool)>();

        private readonly Dictionary<Unit, NodeInfo> _nodeInfoCache = new Dictionary<Unit, NodeInfo>();

        private struct NodeInfo
        {
            public NodeGenerator Generator;
            public bool IsEvent;
            public bool IsUpdate;
            public bool IsCoroutine;
        }

        protected abstract FlowGraph GetFlowGraph();
        protected abstract GraphPointer GetGraphPointer();
        protected abstract string GetGraphName();

        protected virtual bool IsValid() => true;

        public override ControlGenerationData CreateGenerationData()
        {
            return new ControlGenerationData(typeof(MonoBehaviour), GetGraphPointer());
        }

        public override void Generate(CodeWriter writer, ControlGenerationData data)
        {
            if (!IsValid()) return;

            var graphName = GetGraphName();
            if (string.IsNullOrEmpty(graphName)) return;

            CodeBuilder.Indent(1);

            base.data = data;

            var @class = ClassGenerator.Class(RootAccessModifier.Public, ClassModifier.None, graphName, typeof(MonoBehaviour));

            if (GetFlowGraph().units.Any(u => u is GraphInput || u is GraphOutput))
            {
                @class.beforeUsingsAction += (w) => w.WriteWarningDiagnostic("Direct generation of subgraphs will ignore the GraphInput and GraphOutput ports", "Warning: This graph appears to be a subgraph.", WriteOptions.NewLineAfter);
                @class.beforeUsingsAction += (w) => w.Write("#pragma warning disable".ConstructHighlight(), WriteOptions.NewLineAfter);
            }

            @class.generateUsings = true;

            Initialize(@class);
            GenerateVariableDeclarations(@class);
            GenerateEventMethods(@class);
            GenerateSpecialUnits(@class);

            foreach (var kvp in RequiredMethods)
            {
                var name = kvp.Key;
                var method = kvp.Value.Item1;
                var createNew = kvp.Value.Item2;

                if (!createNew) continue;
                @class.AddMethod(method);
            }

            var values = CodeGeneratorValueUtility.GetAllValues(GetGraphPointer().rootObject);
            var index = 0;
            foreach (var variable in values)
            {
                var field = FieldGenerator.Field(AccessModifier.Public, FieldModifier.None, variable.Value != null ? variable.Value.GetType() : typeof(UnityEngine.Object), data.AddLocalNameInScope(variable.Key.LegalMemberName()));

                var attribute = AttributeGenerator.Attribute(typeof(FoldoutAttribute));
                attribute.AddParameter("ObjectReferences");
                field.AddAttribute(attribute);

                @class.AddField(field);
                index++;
            }

            @class.Generate(writer, data);

            data.Dispose();
        }

        private void Initialize(ClassGenerator @class)
        {
            customEventId = 0;
            _customEventIds = new Dictionary<CustomEvent, int>(2);
            _namedAnimationEventIds = new Dictionary<BoltNamedAnimationEvent, int>(2);
            _unityEventIds = new Dictionary<BoltUnityEvent, int>(2);
            _eventUnits = new List<Unit>();
            _updateUnits.Clear();
            _generatorCount.Clear();
            focusTrueUnits.Clear();
            focusFalseUnits.Clear();
            pauseTrueUnits.Clear();
            pauseFalseUnits.Clear();
            RequiredMethods.Clear();

            _allUnits = new List<Unit>();

            GraphTraversal.TraverseFlowGraph(GetFlowGraph(), (unit) =>
            {
                var generator = unit.GetGenerator();
                var info = new NodeInfo
                {
                    Generator = generator,
                    IsEvent = unit is IEventUnit,
                    IsUpdate = generator is IRequireUpdateCode,
                    IsCoroutine = unit is IEventUnit e && e.coroutine
                };

                _nodeInfoCache[unit] = info;

                if (generator is IRequireVariables variables)
                {
                    foreach (var variable in variables.GetRequiredVariables(data))
                    {
                        variable.SetOwner(unit);
                        @class.AddField(variable);
                    }
                }

                if (generator is IRequireMethods methods)
                {
                    foreach (var method in methods.GetRequiredMethods(data))
                    {
                        method.SetOwner(unit);
                        if (!RequiredMethods.TryGetValue(method.name, out var body))
                        {
                            body.method = method;
                            body.createNew = true;
                            RequiredMethods[method.name] = body;
                        }
                        else
                        {
                            var key = method.name;
                            var entry = RequiredMethods[key];
                            entry.createNew = true;
                            entry.method.AppendBodyFrom(method);
                            RequiredMethods[key] = entry;
                        }
                    }
                }

                if (generator is CountGenerator countGenerator)
                {
                    var unitType = unit.GetType();
                    if (!_generatorCount.ContainsKey(unitType))
                    {
                        _generatorCount[unitType] = 0;
                    }
                    countGenerator.count = _generatorCount[unitType];
                    _generatorCount[unitType]++;
                }

                GenerateAwakeHandlers(@class, unit);

                if (generator is IRequireUpdateCode)
                {
                    _updateUnits.Add(unit);
                }

                if (generator is VariableNodeGenerator variableNodeGenerator)
                {
                    var field = FieldGenerator.Field(variableNodeGenerator.AccessModifier, variableNodeGenerator.FieldModifier, variableNodeGenerator.Type, variableNodeGenerator.Name);

                    field.SetOwner(unit);

                    if (variableNodeGenerator.HasDefaultValue)
                        field.CustomDefault(variableNodeGenerator.DefaultValue.As().Code(variableNodeGenerator.IsNew, variableNodeGenerator.Literal, true, "", variableNodeGenerator.NewLineLiteral, true, false));
                    @class.AddField(field);
                }
#if PACKAGE_INPUT_SYSTEM_EXISTS && !PACKAGE_INPUT_SYSTEM_1_2_0_OR_NEWER_EXISTS
                else if (unit is OnInputSystemEvent eventUnit && generator is MethodNodeGenerator methodNodeGenerator)
                {
                    if (!eventUnit.trigger.hasValidConnection) continue;
                    var field = FieldGenerator.Field(AccessModifier.Private, FieldModifier.None, typeof(bool), GetInputSystemEventVariableName(unit as OnInputSystemEvent, methodNodeGenerator));
                    field.SetOwner(unit);
                    @class.AddField(field);
                }
#endif
                if (unit is IEventUnit iEvent)
                {
                    if (iEvent is OnApplicationFocus focusEvent)
                    {
                        focusTrueUnits.Add(focusEvent);
                    }
                    else if (iEvent is OnApplicationLostFocus lostFocusEvent)
                    {
                        focusFalseUnits.Add(lostFocusEvent);
                    }
                    else if (iEvent is OnApplicationPause pauseEvent)
                    {
                        pauseTrueUnits.Add(pauseEvent);
                    }
                    else if (iEvent is OnApplicationResume resumeEvent)
                    {
                        pauseFalseUnits.Add(resumeEvent);
                    }
                }

                if (generator is InterfaceNodeGenerator interfaceNodeGenerator)
                {
                    foreach (var interfaceType in interfaceNodeGenerator.InterfaceTypes)
                    {
                        @class.ImplementInterface(interfaceType);
                    }
                }

                foreach (var @namespace in generator.GetNamespaces())
                {
                    @class.AddUsing(@namespace);
                }

                _allUnits.Add(unit);

                if (generator is UpdateMethodNodeGenerator) return;

                if (unit is CustomEvent evt && evt.graph != GetFlowGraph()) return;

                if (unit is ReturnEvent) return;

                if (_nodeInfoCache[unit].IsEvent || generator is MethodNodeGenerator) _eventUnits.Add(unit);
            });
        }

        private void GenerateVariableDeclarations(ClassGenerator @class)
        {
            foreach (VariableDeclaration variable in GetFlowGraph().variables)
            {
#if VISUAL_SCRIPTING_1_7
                var type = !string.IsNullOrEmpty(variable.typeHandle.Identification) ? Type.GetType(variable.typeHandle.Identification) : typeof(object);
#else
                var type = variable.value?.GetType() ?? typeof(object);
#endif
                var isDelegate = typeof(IDelegate).IsAssignableFrom(type);
                type = isDelegate ? (variable.value as IDelegate)?.GetDelegateType() ?? (Activator.CreateInstance(type) as IDelegate)?.GetDelegateType() : type;

                var modifer = CSharpPreviewWindow.isPreviewing ? AccessModifier.Private : AccessModifier.Public;

                var field = FieldGenerator.Field(modifer, FieldModifier.None, type, variable.name.LegalVariableName(), variable.value);

                if (!CSharpPreviewWindow.isPreviewing)
                {
                    field.sameLineAttributes = true;
                    field.AddAttribute(AttributeGenerator.Attribute<HideInInspector>());
                }

                if (isDelegate)
                {
                    field.Default(null);
                }

                field.SetNewlineLiteral(false);
                @class.AddField(field);
            }
        }

#if PACKAGE_INPUT_SYSTEM_EXISTS && !PACKAGE_INPUT_SYSTEM_1_2_0_OR_NEWER_EXISTS
        private string GetInputSystemEventVariableName(OnInputSystemEvent onInputSystemEvent, MethodNodeGenerator methodNodeGenerator)
        {
            if (onInputSystemEvent is OnInputSystemEventButton)
            {
                return $"button{methodNodeGenerator.count}_wasRunning";
            }
            else if (onInputSystemEvent is OnInputSystemEventFloat)
            {
                return $"float{methodNodeGenerator.count}_wasRunning";
            }
            else
            {
                return $"vector2{methodNodeGenerator.count}_wasRunning";
            }
        }
#endif

        int customEventId = 0;
        private void GenerateAwakeHandlers(ClassGenerator @class, Unit unit)
        {
            bool hasAwakeMethod = @class.methods.Any(m => m.name == "Awake");
            var awakeMethod = !hasAwakeMethod ? MethodGenerator.Method(AccessModifier.Private, MethodModifier.None, typeof(void), "Awake") : @class.methods.First(m => m.name == "Awake");
            bool addMethod = false;
            if (unit is CustomEvent eventUnit)
            {
                if (eventUnit.graph != GetFlowGraph()) return;
                data.SetReturns(_nodeInfoCache[eventUnit].IsCoroutine ? typeof(IEnumerator) : typeof(void));
                data.AddLocalNameInScope("args", typeof(CustomEventArgs));
                _customEventIds.Add(eventUnit, customEventId);
                customEventId++;

                var eventName = GetMethodName(eventUnit) + "Runner";
                awakeMethod.bodyAction += delegate (CodeWriter w)
                {
                    using (w.BeginNode(eventUnit))
                    {
                        w.WriteIndented();
                        w.CSharpUtilityType().Write(".RegisterCustomEvent(");
                        eventUnit.GenerateValue(eventUnit.target, w, data);
                        w.Write($", {eventName}");
                        w.Write($", {(eventName + "_" + eventUnit.ToString().Replace(".", "")).As().Code(false)});");
                        w.NewLine();
                    }
                };

                var runnerMethod = MethodGenerator.Method(AccessModifier.Private, MethodModifier.None, typeof(void), eventName);
                runnerMethod.SetOwner(unit);
                runnerMethod.AddParameter(ParameterGenerator.Parameter("args", typeof(CustomEventArgs), ParameterModifier.None));
                runnerMethod.Body(delegate (CodeWriter writer)
                {
                    using (writer.BeginNode(eventUnit))
                        WriteCustomEventRunnerCode(eventUnit, data, writer);
                });

                @class.AddMethod(runnerMethod);

                var generator = eventUnit.GetMethodGenerator();

                var method = MethodGenerator.Method(generator.AccessModifier, generator.MethodModifier, generator.ReturnType, GetMethodName(eventUnit, true));
                AddMethodAttributes(method, generator.Attributes);
                AddMethodParameters(method, generator);
                method.Body((w) =>
                {
                    data.SetReturns(generator.ReturnType);
                    generator.GenerateControl(null, data, w);
                });
                method.SetOwner(eventUnit);
                @class.AddMethod(method);

                addMethod = true;
            }
            else if (_nodeInfoCache[unit].Generator is IRequireAwakeCode requireAwakeCode)
            {
                Action<CodeWriter> body = null;

                data.SetReturns(typeof(void));
                body += (w) =>
                {
                    using (w.BeginNode(unit))
                    {
                        requireAwakeCode.GenerateAwakeCode(data, w);
                    }
                };

                addMethod = true;

                awakeMethod.bodyAction += body;
            }

            if (!hasAwakeMethod && addMethod)
                @class.AddMethod(awakeMethod);
        }

        private void GenerateEventMethods(ClassGenerator @class)
        {
            var methodBodies = new Dictionary<string, MethodGenerator>();
            var coroutineBodies = new Dictionary<string, MethodGenerator>();

            bool addedSpecialUpdateCode = false;
#pragma warning disable
            bool addedSpecialFixedUpdateCode = false;
#pragma warning restore

            if (focusTrueUnits.Count > 0 || focusFalseUnits.Count > 0)
            {
                var method = MethodGenerator.Method(AccessModifier.Private, MethodModifier.None, typeof(void), "OnApplicationFocus");
                method.AddParameter(ParameterGenerator.Parameter("focus", typeof(bool), ParameterModifier.None));

                method.Body(delegate (CodeWriter writer)
                {
                    if (focusTrueUnits.Count > 0)
                    {
                        data.SetReturns(typeof(void));
                        writer.WriteIndented("if".ControlHighlight()).Parentheses(inner => inner.Write("focus".VariableHighlight()));
                        writer.Braces((inner, _indent) =>
                        {
                            for (int i = 0; i < focusTrueUnits.Count; i++)
                            {
                                var unit = focusTrueUnits[i];
                                if (i != 0) writer.NewLine();
                                WriteMethodBody(unit, data, inner);
                            }
                        });
                    }

                    if (focusFalseUnits.Count > 0)
                    {
                        data.SetReturns(typeof(void));
                        writer.WriteIndented("else".ControlHighlight());
                        writer.Braces((inner, _indent) =>
                        {
                            for (int i = 0; i < focusFalseUnits.Count; i++)
                            {
                                var unit = focusFalseUnits[i];
                                if (i != 0) writer.NewLine();
                                WriteMethodBody(unit, data, inner);
                            }
                        });
                    }
                });

                @class.AddMethod(method);
            }

            if (pauseTrueUnits.Count > 0 || pauseFalseUnits.Count > 0)
            {
                var method = MethodGenerator.Method(AccessModifier.Private, MethodModifier.None, typeof(void), "OnApplicationPause");
                method.AddParameter(ParameterGenerator.Parameter("paused", typeof(bool), ParameterModifier.None));

                method.Body(delegate (CodeWriter writer)
                {
                    if (pauseTrueUnits.Count > 0)
                    {
                        data.SetReturns(typeof(void));
                        writer.WriteIndented("if".ControlHighlight()).Parentheses(inner => inner.Write("paused".VariableHighlight()));
                        writer.Braces((inner, _indent) =>
                        {
                            for (int i = 0; i < pauseTrueUnits.Count; i++)
                            {
                                var unit = pauseTrueUnits[i];
                                if (i != 0) writer.NewLine();
                                WriteMethodBody(unit, data, inner);
                            }
                        });
                    }

                    if (pauseFalseUnits.Count > 0)
                    {
                        data.SetReturns(typeof(void));
                        writer.WriteIndented("else".ControlHighlight());
                        writer.Braces((inner, _indent) =>
                        {
                            for (int i = 0; i < pauseFalseUnits.Count; i++)
                            {
                                var unit = pauseFalseUnits[i];
                                if (i != 0) writer.NewLine();
                                WriteMethodBody(unit, data, inner);
                            }
                        });
                    }
                });

                @class.AddMethod(method);
            }

            foreach (Unit Unit in _eventUnits.OrderBy(unit => GetUnitOrder(unit)))
            {
                if (Unit is OnApplicationFocus ||
                    Unit is OnApplicationLostFocus ||
                    Unit is OnApplicationPause ||
                    Unit is OnApplicationResume ||
                    Unit is CustomEvent)
                {
                    continue;
                }
                if (Unit is IEventUnit unit)
                {
                    if (Unit is TriggerReturnEvent triggerReturn)
                    {
                        HandleMethodNodeGenerator(triggerReturn.GetMethodGenerator(), methodBodies);
                        continue;
                    }
                    string unityMethodName = GetMethodName(unit);
                    var parameters = GetMethodParameters(unit);

                    if (RequiredMethods.TryGetValue(unityMethodName, out var value))
                    {
                        value.createNew = false;
                        RequiredMethods[unityMethodName] = value;
                    }

                    Action<CodeWriter> specialUnitCode = null;
                    bool isUpdate = unit is Update;
                    bool isFixedUpdate = unit is FixedUpdate;
                    bool isCoroutine = unit.coroutine;

                    if (isUpdate && !addedSpecialUpdateCode)
                    {
                        addedSpecialUpdateCode = true;
                        specialUnitCode = GenerateSpecialUnitCode(@class, true);
                    }
#if PACKAGE_INPUT_SYSTEM_EXISTS
                    else if (isFixedUpdate && !addedSpecialFixedUpdateCode && UnityEngine.InputSystem.InputSystem.settings.updateMode != UnityEngine.InputSystem.InputSettings.UpdateMode.ProcessEventsInDynamicUpdate)
                    {
                        addedSpecialFixedUpdateCode = true;
                        specialUnitCode = GenerateSpecialUnitCode(@class, false);
                    }
#endif

                    if (isCoroutine)
                    {
                        string coroutineMethodName = GetMethodName(unit, true);

                        var generator = Unit.GetMethodGenerator(false);

                        data.SetReturns(typeof(IEnumerator));
                        bool HasReturned = data.HasReturned;
                        if (!coroutineBodies.TryGetValue(coroutineMethodName, out var coroutineMethod))
                        {
                            coroutineMethod = MethodGenerator.Method(AccessModifier.Private, MethodModifier.None, typeof(IEnumerator), coroutineMethodName);
                            coroutineMethod.SetOwner(Unit);
                            if (generator != null)
                            {
                                AddMethodParameters(coroutineMethod, generator);
                            }
                            coroutineBodies[coroutineMethodName] = coroutineMethod;
                        }

                        coroutineMethod.bodyAction += delegate (CodeWriter writer)
                        {
                            WriteMethodBody(unit, data, writer, unityMethodName);
                        };

                        if (!methodBodies.TryGetValue(unityMethodName, out var unityMethod))
                        {
                            unityMethod = MethodGenerator.Method(AccessModifier.Private, MethodModifier.None, typeof(void), unityMethodName);
                            unityMethod.SetOwner(Unit);
                            if (Unit is BoltNamedAnimationEvent || Unit is BoltAnimationEvent || Unit is BoltUnityEvent)
                            {
                                unityMethod.SetSummary($"Handles the linked {Unit.GetType().Name} event logic.\nUse this method when assigning the event callback in the Unity Inspector.");
                                if (Unit is BoltNamedAnimationEvent namedAnimationEvent && namedAnimationEvent.name.hasValidConnection)
                                {
                                    unityMethod.SetWarning("Note: Dynamic event names (e.g., the name port being connected) are not supported. This will only generate the method for the first resolved name at generation time.");
                                }
                                else if (Unit is BoltUnityEvent unityEvent && unityEvent.name.hasValidConnection)
                                {
                                    unityMethod.SetWarning("Note: Dynamic event names (e.g., the name port being connected) are not supported. This will only generate the method for the first resolved name at generation time.");
                                }
                            }
                            if (generator != null)
                            {
                                AddMethodParameters(unityMethod, generator);
                                AddMethodAttributes(unityMethod, generator.Attributes);
                            }
                            methodBodies[unityMethodName] = unityMethod;

                            if (specialUnitCode != null)
                                unityMethod.bodyAction += specialUnitCode;
                        }
                        if (Unit is BoltNamedAnimationEvent animationEvent)
                        {
                            unityMethod.bodyAction += delegate (CodeWriter writer)
                            {
                                using (writer.BeginNode(animationEvent))
                                {
                                    writer.WriteIndented("if ".ControlHighlight()).Parentheses(inner =>
                                    {
                                        inner.GetMember("animationEvent".VariableHighlight(), "stringParameter").Equals();
                                        animationEvent.GenerateValue(animationEvent.name, inner, data);
                                    });

                                    writer.Braces((inner, _indent) =>
                                    {
                                        inner.WriteIndented("StartCoroutine").Parentheses(inside => inside.Write(coroutineMethodName).Parentheses(_inside =>
                                        {
                                            if (generator != null)
                                            {
                                                _inside.Write(string.Join(", ", generator.Parameters.Select(p => p.name.VariableHighlight())));
                                            }
                                        })).Write(";");
                                    }).NewLine();
                                }
                            };
                        }
                        else
                        {
                            unityMethod.bodyAction += delegate (CodeWriter writer)
                            {
                                using (writer.BeginNode(Unit))
                                {
                                    writer.WriteIndented($"StartCoroutine({coroutineMethodName}({(generator != null ? string.Join(", ", generator.Parameters.Select(p => p.name.VariableHighlight())) : "")}));");
                                    writer.NewLine();
                                }
                            };
                        }
                    }
                    else
                    {
                        var generator = Unit.GetMethodGenerator(false);
                        data.SetReturns(typeof(void));

                        if (!methodBodies.TryGetValue(unityMethodName, out var method))
                        {
                            method = MethodGenerator.Method(AccessModifier.Private, MethodModifier.None, typeof(void), unityMethodName);
                            method.SetOwner(Unit);
                            if (Unit is BoltNamedAnimationEvent || Unit is BoltAnimationEvent || Unit is BoltUnityEvent)
                            {
                                method.SetSummary($"Handles the linked {Unit.GetType().Name} event logic.\nUse this method when assigning the event callback in the Unity Inspector.");
                                if (Unit is BoltNamedAnimationEvent namedAnimationEvent && namedAnimationEvent.name.hasValidConnection)
                                {
                                    method.SetWarning("Note: Dynamic event names (e.g., the name port being connected) are not supported. This will only generate the method for the first resolved name at generation time.");
                                }
                                else if (Unit is BoltUnityEvent unityEvent && unityEvent.name.hasValidConnection)
                                {
                                    method.SetWarning("Note: Dynamic event names (e.g., the name port being connected) are not supported. This will only generate the method for the first resolved name at generation time.");
                                }
                            }
                            if (generator != null)
                            {
                                AddMethodParameters(method, generator);
                                AddMethodAttributes(method, generator.Attributes);
                            }
                            methodBodies[unityMethodName] = method;

                            if (specialUnitCode != null)
                                method.bodyAction += specialUnitCode;
                        }
                        if (Unit is BoltNamedAnimationEvent animationEvent)
                        {
                            methodBodies[unityMethodName].bodyAction += delegate (CodeWriter writer)
                            {
                                using (writer.BeginNode(animationEvent))
                                {
                                    writer.WriteIndented("if ".ControlHighlight()).Parentheses(inner =>
                                    {
                                        inner.GetMember("animationEvent".VariableHighlight(), "stringParameter").Equals();
                                        animationEvent.GenerateValue(animationEvent.name, inner, data);
                                    });

                                    writer.Braces((inner, _indent) =>
                                    {
                                        WriteMethodBody(unit, data, writer);
                                    }).NewLine();
                                }
                            };
                        }
                        else
                            methodBodies[unityMethodName].bodyAction += delegate (CodeWriter writer)
                            {
                                WriteMethodBody(unit, data, writer, unityMethodName);
                            };
                    }
                }
                else if (_nodeInfoCache[Unit].Generator is MethodNodeGenerator methodNodeGenerator)
                {
                    HandleMethodNodeGenerator(methodNodeGenerator, methodBodies);
                }
            }

            foreach (var method in methodBodies.Values)
                @class.AddMethod(method);

            foreach (var coroutine in coroutineBodies.Values)
                @class.AddMethod(coroutine);
        }

        private void HandleMethodNodeGenerator(MethodNodeGenerator methodNodeGenerator, Dictionary<string, MethodGenerator> methodBodies)
        {
            void body(CodeWriter w)
            {
                data.SetReturns(methodNodeGenerator.ReturnType);
                try
                {
                    using (w.BeginNode(methodNodeGenerator.unit))
                    {
                        methodNodeGenerator.GeneratedMethodCode(data, w);
                    }
                }
                catch (NotImplementedException)
                {
                    methodNodeGenerator.GenerateControl(null, data, w);
                }
            }
            if (!methodBodies.TryGetValue(methodNodeGenerator.Name, out var method))
            {
                method = MethodGenerator.Method(methodNodeGenerator.AccessModifier, methodNodeGenerator.MethodModifier, methodNodeGenerator.ReturnType, methodNodeGenerator.Name);
                method.SetOwner(methodNodeGenerator.unit);
                method.AddGenerics(methodNodeGenerator.GenericCount);
                foreach (var param in methodNodeGenerator.Parameters)
                {
                    if (methodNodeGenerator.GenericCount == 0 || !param.usesGeneric)
                        method.AddParameter(ParameterGenerator.Parameter(param.name, param.type, param.modifier));
                    else if (methodNodeGenerator.GenericCount > 0 && param.usesGeneric)
                    {
                        var genericString = method.generics[param.generic].name;
                        method.AddParameter(ParameterGenerator.Parameter(param.name, genericString.TypeHighlight(), param.type, param.modifier));
                    }
                }
                methodBodies[methodNodeGenerator.Name] = method;
            }

            if (RequiredMethods.TryGetValue(methodNodeGenerator.Name, out var _body))
            {
                methodBodies[methodNodeGenerator.Name].bodyAction += body;
            }
            else
            {
                methodBodies[methodNodeGenerator.Name].bodyAction += body;
            }
        }

        private void AddMethodAttributes(MethodGenerator method, List<AttributeDeclaration> attributes)
        {
            if (attributes.Count > 0)
            {
                foreach (var attribute in attributes)
                {
                    var attrGenerator = AttributeGenerator.Attribute(attribute.GetAttributeType());
                    foreach (var param in attribute.parameters)
                    {
                        if (param.defaultValue is IList list)
                        {
                            if (param.modifier == ParameterModifier.Params)
                            {
                                foreach (var item in list)
                                {
                                    attrGenerator.AddParameter(item);
                                }
                            }
                            else if (!attrGenerator.parameterValues.Contains(param))
                            {
                                attrGenerator.AddParameter(param.defaultValue);
                            }
                        }
                        else if (!attrGenerator.parameterValues.Contains(param))
                        {
                            attrGenerator.AddParameter(param.defaultValue);
                        }
                    }
                    foreach (var item in attribute.fields)
                    {
                        attrGenerator.AddParameter(item.Key, item.Value);
                    }
                    method.AddAttribute(attrGenerator);
                }
            }
        }

        private void AddMethodParameters(MethodGenerator method, MethodNodeGenerator methodNodeGenerator)
        {
            foreach (var param in methodNodeGenerator.Parameters)
            {
                if (methodNodeGenerator.GenericCount == 0 || !param.usesGeneric)
                    method.AddParameter(ParameterGenerator.Parameter(param.name, param.type, param.modifier));
                else if (methodNodeGenerator.GenericCount > 0 && param.usesGeneric)
                {
                    var genericString = method.generics[param.generic].name;
                    method.AddParameter(ParameterGenerator.Parameter(param.name, genericString.TypeHighlight(), param.type, param.modifier));
                }
                data.AddLocalNameInScope(param.name, param.type);
            }
        }

        private static readonly Dictionary<Type, int> EventOrder = new Dictionary<Type, int>()
        {
            { typeof(OnEnable), 0 },
            { typeof(Start), 1 },
            { typeof(FixedUpdate), 2 },
            { typeof(Update), 3 },
            { typeof(LateUpdate), 4 }
        };

        private int GetUnitOrder(Unit unit)
        {
            return EventOrder.TryGetValue(unit.GetType(), out var order) ? order : 5;
        }

        private Action<CodeWriter> GenerateSpecialUnitCode(ClassGenerator @class, bool isUpdate)
        {
            Action<CodeWriter> code = null;

            if (isUpdate)
            {
                foreach (var unit in _updateUnits)
                {
                    var generator = _nodeInfoCache[unit].Generator;
                    if (generator is IRequireUpdateCode updateCode)
                    {
                        code += (w) =>
                        {
                            using (w.BeginNode(unit))
                            {
                                updateCode.GenerateUpdateCode(data, w);
                            }
                        };
                    }
                }
            }

#if PACKAGE_INPUT_SYSTEM_EXISTS
            if (isUpdate)
            {
                if (UnityEngine.InputSystem.InputSystem.settings.updateMode == InputSettings.UpdateMode.ProcessEventsInDynamicUpdate)
                {
                    foreach (var inputUnit in _allUnits.OfType<OnInputSystemEvent>())
                    {
                        if (!inputUnit.trigger.hasValidConnection) continue;
                        code += (w) =>
                        {
                            using (w.BeginNode(inputUnit))
                            {
                                w.WriteIndented(inputUnit.GetMethodGenerator().Name + "();");
                                w.NewLine();
                            }
                        };
                    }
                }
            }
            else
            {
                foreach (var inputUnit in _allUnits.OfType<OnInputSystemEvent>())
                {
                    if (!inputUnit.trigger.hasValidConnection) continue;
                    code += (w) =>
                    {
                        using (w.BeginNode(inputUnit))
                        {
                            w.WriteIndented(inputUnit.GetMethodGenerator().Name + "();");
                            w.NewLine();
                        }
                    };
                }
            }
#endif

            return code;
        }

        private void GenerateSpecialUnits(ClassGenerator @class)
        {
            bool hasUpdate = false;
#pragma warning disable
            bool hasFixedUpdate = false;
#pragma warning restore
#if PACKAGE_INPUT_SYSTEM_EXISTS
            bool hasInputSystemNode = false;
#endif
            foreach (var unit in _updateUnits.Where(u => _nodeInfoCache[u].Generator is UpdateMethodNodeGenerator))
            {
                var generator = unit.GetMethodGenerator();
                var method = MethodGenerator.Method(generator.AccessModifier, generator.MethodModifier, generator.ReturnType, generator.Name);
                method.SetOwner(unit);
                foreach (var param in generator.Parameters)
                {
                    method.AddParameter(ParameterGenerator.Parameter(param.name, param.type, param.modifier, param.hasDefault, param.defaultValue));
                }

                void MethodBody(CodeWriter w)
                {
                    data.SetReturns(generator.ReturnType);

                    generator.GenerateControl(null, data, w);
                }
                method.Body(MethodBody);
                @class.AddMethod(method);
            }
            foreach (var unit in _eventUnits)
            {
                if (unit is Update) { hasUpdate = true; continue; }
                if (unit is FixedUpdate) { hasFixedUpdate = true; continue; }
#if PACKAGE_INPUT_SYSTEM_EXISTS
                if (unit is OnInputSystemEvent onInputSystemEvent && onInputSystemEvent.trigger.hasValidConnection) { hasInputSystemNode = true; continue; }
#endif
            }
            if (_updateUnits.Count > 0 && !hasUpdate)
            {
                var updateMethod = MethodGenerator.Method(AccessModifier.Private, MethodModifier.None, typeof(void), "Update");

                updateMethod.Body(delegate (CodeWriter writer)
                {
                    foreach (var unit in _updateUnits)
                    {
                        var generator = _nodeInfoCache[unit].Generator;
                        if (generator is IRequireUpdateCode updateCode)
                        {
                            using (writer.BeginNode(unit))
                                updateCode.GenerateUpdateCode(data, writer);
                        }
                    }

#if PACKAGE_INPUT_SYSTEM_EXISTS
                    if (UnityEngine.InputSystem.InputSystem.settings.updateMode == InputSettings.UpdateMode.ProcessEventsInDynamicUpdate)
                    {
                        foreach (var unit in _allUnits.OfType<OnInputSystemEvent>())
                        {
                            if (!unit.trigger.hasValidConnection) continue;

                            using (writer.BeginNode(unit))
                            {
                                writer.Write(unit.GetMethodGenerator().Name + "();");
                                writer.NewLine();
                            }
                        }
                    }
#endif
                });
                @class.AddMethod(updateMethod);
            }

#if PACKAGE_INPUT_SYSTEM_EXISTS
            else if (UnityEngine.InputSystem.InputSystem.settings.updateMode == InputSettings.UpdateMode.ProcessEventsInDynamicUpdate
                     && !hasUpdate
                     && hasInputSystemNode)
            {
                var updateMethod = MethodGenerator.Method(AccessModifier.Private, MethodModifier.None, typeof(void), "Update");

                updateMethod.Body(delegate (CodeWriter writer)
                        {
                            foreach (var unit in _allUnits.OfType<OnInputSystemEvent>())
                            {
                                if (!unit.trigger.hasValidConnection) continue;

                                using (writer.BeginNode(unit))
                                {
                                    writer.Write(unit.GetMethodGenerator().Name + "();");
                                    writer.NewLine();
                                }
                            }
                        });

                @class.AddMethod(updateMethod);
            }
            else if (UnityEngine.InputSystem.InputSystem.settings.updateMode != InputSettings.UpdateMode.ProcessEventsInDynamicUpdate
                     && !hasFixedUpdate
                     && hasInputSystemNode)
            {
                var fixedMethod = MethodGenerator.Method(AccessModifier.Private, MethodModifier.None, typeof(void), "FixedUpdate");
                fixedMethod.Body(delegate (CodeWriter writer)
                {
                    foreach (var unit in _allUnits.OfType<OnInputSystemEvent>())
                    {
                        if (!unit.trigger.hasValidConnection) continue;

                        using (writer.BeginNode(unit))
                        {
                            writer.Write(unit.GetMethodGenerator().Name + "();");
                            writer.NewLine();
                        }
                    }
                });
                @class.AddMethod(fixedMethod);
            }
#endif
        }

        private void WriteCustomEventRunnerCode(CustomEvent eventUnit, ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented("if ".ControlHighlight()).Parentheses(inner =>
            {
                inner.GetMember("args".VariableHighlight(), "name").Equals();
                eventUnit.GenerateValue(eventUnit.name, inner, data);
            });
            writer.Braces((inner, indent) =>
            {
                if (eventUnit.coroutine)
                    inner.WriteLine($"StartCoroutine(" + GetMethodName(eventUnit, true) + $"({"args".VariableHighlight()}));");
                else
                    inner.WriteLine(GetMethodName(eventUnit) + $"({"args".VariableHighlight()});");
            }).NewLine();
        }

        private string GetMethodName(IEventUnit eventUnit, bool getCoroutine = false)
        {
            var UnitTitle = BoltFlowNameUtility.UnitTitle(eventUnit.GetType(), false, false).LegalMemberName();

            string methodName;

            if (EVENT_NAMES.TryGetValue(UnitTitle, out var title))
            {
                methodName = title;
            }
            else
            {
                methodName = UnitTitle;
            }

            if (eventUnit is CustomEvent customEvent)
            {
                if (!customEvent.name.hasValidConnection)
                {
                    methodName = ((string)customEvent.defaultValues[customEvent.name.key]).LegalMemberName();
                    if (string.IsNullOrEmpty(methodName)) return "CustomEvent" + _customEventIds[customEvent];
                }
                else
                {
                    if (NodeGenerator.CanPredictConnection(customEvent.name, data))
                    {
                        data.TryGetGraphPointer(out var graphPointer);
                        methodName = Flow.Predict<string>(customEvent.name, graphPointer.AsReference()).LegalMemberName();
                    }
                    else
                        return "CustomEvent" + _customEventIds[customEvent];
                }
            }
            else if (eventUnit is BoltNamedAnimationEvent animationEvent)
            {
                if (NodeGenerator.CanPredictConnection(animationEvent.name, data))
                {
                    data.TryGetGraphPointer(out var graphPointer);
                    methodName = Flow.Predict<string>(animationEvent.name, graphPointer.AsReference()) + "_AnimationEvent";
                }
                else if (!animationEvent.name.hasValidConnection)
                    methodName = animationEvent.defaultValues[animationEvent.name.key] as string + "_AnimationEvent";
                else
                {
                    if (!_namedAnimationEventIds.TryGetValue(animationEvent, out int count))
                    {
                        count = 0;
                    }
                    _namedAnimationEventIds[animationEvent] = count + 1;
                    methodName = "AnimationEvent" + _namedAnimationEventIds[animationEvent];
                }
                return methodName + (getCoroutine && eventUnit.coroutine ? "_Coroutine" : "");
            }
            else if (eventUnit is BoltUnityEvent unityEvent)
            {
                if (NodeGenerator.CanPredictConnection(unityEvent.name, data))
                {
                    data.TryGetGraphPointer(out var graphPointer);
                    methodName = Flow.Predict<string>(unityEvent.name, graphPointer.AsReference()) + "_UnityEvent";
                }
                else if (!unityEvent.name.hasValidConnection)
                    methodName = unityEvent.defaultValues[unityEvent.name.key] as string + "_UnityEvent";
                else
                {
                    if (!_unityEventIds.TryGetValue(unityEvent, out int count))
                    {
                        count = 0;
                    }
                    _unityEventIds[unityEvent] = count + 1;
                    methodName = "UnityEvent" + _unityEventIds[unityEvent];
                }
                return methodName + (getCoroutine && eventUnit.coroutine ? "_Coroutine" : "");
            }
            else if ((eventUnit as Unit).GetMethodGenerator(false) is MethodNodeGenerator methodNodeGenerator)
            {
                return methodNodeGenerator.Name + (getCoroutine && eventUnit.coroutine ? "_Coroutine" : "");
            }

            return methodName + (getCoroutine && eventUnit.coroutine ? "_Coroutine" : "");
        }

        private void WriteMethodBody(IEventUnit eventUnit, ControlGenerationData data, CodeWriter writer, string methodName = "")
        {
            if (RequiredMethods.TryGetValue(methodName, out var entry))
            {
                entry.method.bodyAction += w =>
                {
                    (eventUnit as Unit).GenerateControl(null, data, w);
                };
            }
            else
            {
                (eventUnit as Unit).GenerateControl(null, data, writer);
            }
        }

        private List<TypeParam> GetMethodParameters(IEventUnit eventUnit)
        {
            return (eventUnit as Unit).GetMethodGenerator(false)?.Parameters ?? new List<TypeParam>();
        }
    }
}