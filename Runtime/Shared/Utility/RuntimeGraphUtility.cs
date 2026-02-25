using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.Humility;

#if VISUAL_SCRIPTING_1_7
using SUnit = Unity.VisualScripting.SubgraphUnit;
#else
using SUnit = Unity.VisualScripting.SuperUnit;
#endif
namespace Unity.VisualScripting.Community
{
    public static class RuntimeGraphUtility
    {
        /// <summary>
        /// Looks for the roots a the flow.
        /// </summary>
        /// <param name="unit">The Unit to start searching from</param>
        /// <param name="reference">Optional: if null it will not check parent graphs if there is one</param>
        /// <returns>All the root's that were found</returns>
        public static IEnumerable<Unit> GetFlowSourceUnits(Unit unit, GraphReference reference = null)
        {
            return GetFlowSourceUnitsInternal(unit, new HashSet<Unit>(), reference);
        }

        private static IEnumerable<Unit> GetFlowSourceUnitsInternal(Unit unit, HashSet<Unit> visited = null, GraphReference reference = null,
        string key = null)
        {
            visited ??= new HashSet<Unit>();

            if (unit == null)
                yield break;

            if (visited.Contains(unit))
                yield break;

            if (unit is INesterUnit nesterUnit && reference != null && nesterUnit.nest != null && nesterUnit.nest.graph is FlowGraph graph)
            {
                if (graph.units.FirstOrDefault(e => e is GraphOutput) is GraphOutput graphOutput)
                {
                    var correspondingControlInput = graphOutput.controlInputs.FirstOrDefault(c => c.key == key);
                    if (correspondingControlInput != null && correspondingControlInput.hasValidConnection)
                    {
                        foreach (var conn in correspondingControlInput.connections)
                        {
                            if (conn?.source?.unit is Unit source)
                            {
                                foreach (var upstream in GetFlowSourceUnitsInternal(source, visited, reference.ChildReference(nesterUnit, false), conn.source.key))
                                    yield return upstream;
                            }
                        }

                    }
                    else if (correspondingControlInput != null && !correspondingControlInput.hasValidConnection)
                    {
                        yield return graphOutput;
                    }
                    else
                    {
                        foreach (var upstream in GetFlowSourceUnitsInternal(nesterUnit as Unit, visited, reference, key))
                            yield return upstream;
                    }
                }
                yield break;
            }

            var flowInputs = unit.controlInputs.Where(i => i.hasValidConnection).ToList();
            if (flowInputs.Count > 0)
            {
                visited.Add(unit);
                foreach (var input in flowInputs)
                {
                    foreach (var conn in input.connections)
                    {
                        if (conn?.source?.unit is Unit source)
                        {
                            foreach (var upstream in GetFlowSourceUnitsInternal(source, visited, reference, conn.source.key))
                            {
                                yield return upstream;
                            }
                        }
                    }
                }

                yield break;
            }

            if (unit is GraphInput gi && reference != null && reference.parentElement is INesterUnit nester)
            {
                var parent = reference.ParentReference(false);

                var correspondingParentInput = nester.controlInputs.FirstOrDefault(ci => ci.key == key);

                if (!string.IsNullOrEmpty(key) && correspondingParentInput != null && correspondingParentInput.hasValidConnection)
                {
                    if (correspondingParentInput != null)
                    {
                        foreach (var conn in correspondingParentInput.connections)
                        {
                            if (conn?.source?.unit is Unit src)
                            {
                                foreach (var upstream in GetFlowSourceUnitsInternal(src, visited, parent, key))
                                    yield return upstream;
                            }
                        }
                    }
                }
                else if (correspondingParentInput != null && !correspondingParentInput.hasValidConnection)
                {
                    yield return nester as Unit;
                }
                else
                {
                    foreach (var upstream in GetFlowSourceUnitsInternal(nester as Unit, visited, parent, key))
                        yield return upstream;
                }

                yield break;
            }

            var valueOutputs = unit.valueOutputs.Where(v => v.hasValidConnection).ToList();
            if (valueOutputs.Count > 0)
            {
                foreach (var output in valueOutputs)
                {
                    foreach (var conn in output.connections)
                    {
                        if (conn?.destination?.unit is Unit destination)
                        {
                            foreach (var upstream in GetFlowSourceUnitsInternal(destination, visited, reference, key))
                                yield return upstream;
                        }
                    }
                }
            }

            visited.Add(unit);
            yield return unit;
        }

        public static List<T> GetUnitsOfType<T>(Unit unit, Func<T, bool> predicate = null, GraphReference reference = null) where T : Unit
        {
            return GetUnitsOfTypeInternal(unit, predicate, new HashSet<Unit>(), reference);
        }

        private static List<T> GetUnitsOfTypeInternal<T>(Unit unit, Func<T, bool> predicate = null, HashSet<Unit> visited = null, GraphReference reference = null, string key = null) where T : Unit
        {
            var results = new List<T>();

            if (unit == null || visited.Contains(unit))
                return results;

            if (!(unit is GraphOutput or INesterUnit))
                visited.Add(unit);

            if (unit is T typedUnit && (predicate == null || predicate(typedUnit)))
                results.Add(typedUnit);

            if (reference != null && (unit is INesterUnit || unit is GraphOutput))
            {
                if (unit is INesterUnit nesterUnit && nesterUnit.nest != null && nesterUnit.nest.graph is FlowGraph graph)
                {
                    if (graph.units.FirstOrDefault(e => e is GraphInput) is GraphInput graphInput)
                    {
                        if (graphInput is T typed && (predicate == null || predicate(typed)))
                            results.Add(typed);

                        var correspondingControlOutput = graphInput.controlOutputs.FirstOrDefault(c => c.key == key);
                        if (correspondingControlOutput != null && correspondingControlOutput.hasValidConnection)
                            results.AddRange(GetUnitsOfTypeInternal<T>(correspondingControlOutput.connection.destination.unit as Unit, predicate, visited, reference.ChildReference(nesterUnit, false), correspondingControlOutput.connection.destination.key));
                    }
                }
                else if (unit is GraphOutput graphOutput && reference.isChild && reference.parentElement is INesterUnit parentNester)
                {
                    var parent = reference.ParentReference(false);

                    var correspondingParentInput = parentNester.controlOutputs.FirstOrDefault(ci => ci.key == key);

                    if (correspondingParentInput != null && correspondingParentInput.hasValidConnection)
                    {
                        results.AddRange(GetUnitsOfTypeInternal<T>(correspondingParentInput.connection.destination.unit as Unit, predicate, visited, parent, correspondingParentInput.connection.destination.key));
                    }
                }
            }
            else
            {
                foreach (var output in unit.controlOutputs)
                {
                    if (!output.hasValidConnection)
                        continue;

                    var connection = output.connection;

                    if (connection?.destination?.unit is Unit destUnit)
                    {
                        if (destUnit is T typed && (predicate == null || predicate(typed)))
                            results.Add(typed);

                        results.AddRange(GetUnitsOfTypeInternal<T>(destUnit, predicate, visited, reference, connection.destination.key));
                    }
                }

                foreach (var input in unit.valueInputs)
                {
                    if (!input.hasValidConnection)
                        continue;

                    var connection = input.connection;

                    if (connection?.source?.unit is Unit sourceUnit)
                    {
                        if (sourceUnit is T typed && (predicate == null || predicate(typed)))
                            results.Add(typed);

                        results.AddRange(GetUnitsOfTypeInternal<T>(sourceUnit, predicate, visited, reference, key));
                    }
                }
            }

            return results;
        }

        public static T GetFirstUnitOfType<T>(Unit unit, Func<T, bool> predicate = null, GraphReference reference = null) where T : Unit
        {
            return GetFirstUnitOfTypeInternal(unit, predicate, new HashSet<Unit>(), reference);
        }

        private static T GetFirstUnitOfTypeInternal<T>(Unit unit, Func<T, bool> predicate = null, HashSet<Unit> visited = null, GraphReference reference = null, string key = null) where T : Unit
        {
            if (unit == null || visited.Contains(unit))
                return null;

            visited.Add(unit);

            if (unit is T typedUnit && (predicate == null || predicate(typedUnit)))
                return typedUnit;

            if (reference != null && (unit is INesterUnit || unit is GraphOutput))
            {
                if (unit is INesterUnit nesterUnit && nesterUnit.nest != null && nesterUnit.nest.graph is FlowGraph graph)
                {
                    if (graph.units.FirstOrDefault(e => e is GraphInput) is GraphInput graphInput)
                    {
                        if (graphInput is T typed && (predicate == null || predicate(typed)))
                            return typed;

                        var correspondingControlOutput = graphInput.controlOutputs.FirstOrDefault(c => c.key == key);

                        if (correspondingControlOutput != null && correspondingControlOutput.hasValidConnection)
                        {
                            var result = GetFirstUnitOfTypeInternal<T>(correspondingControlOutput.connection.destination.unit as Unit, predicate, visited, reference.ChildReference(nesterUnit, false), correspondingControlOutput.connection.destination.key);
                            if (result is T _typed && (predicate == null || predicate(_typed)))
                                return _typed;
                        }
                    }
                }
                else if (unit is GraphOutput graphOutput && reference.isChild && reference.parentElement is INesterUnit parentNester)
                {
                    var parent = reference.ParentReference(false);

                    var correspondingParentInput = parentNester.controlOutputs.FirstOrDefault(ci => ci.key == key);

                    if (correspondingParentInput != null && correspondingParentInput.hasValidConnection)
                    {
                        var result = GetFirstUnitOfTypeInternal<T>(correspondingParentInput.connection.destination.unit as Unit, predicate, visited, reference.ParentReference(false), correspondingParentInput.connection.destination.key);
                        if (result is T _typed && (predicate == null || predicate(_typed)))
                            return _typed;
                    }
                }
            }
            else
            {
                foreach (var output in unit.controlOutputs)
                {
                    if (!output.hasValidConnection)
                        continue;

                    var connection = output.connection;

                    if (connection?.destination?.unit is Unit destUnit)
                    {
                        if (destUnit is T typed && (predicate == null || predicate(typed)))
                            return typed;

                        var result = GetFirstUnitOfTypeInternal<T>(destUnit, predicate, visited, reference, connection.destination.key);
                        if (result is T _typed && (predicate == null || predicate(_typed)))
                            return _typed;
                    }
                }

                foreach (var input in unit.valueInputs)
                {
                    if (!input.hasValidConnection)
                        continue;

                    var connection = input.connection;

                    if (connection?.source?.unit is Unit sourceUnit)
                    {
                        if (sourceUnit is T typed && (predicate == null || predicate(typed)))
                            return typed;

                        var result = GetFirstUnitOfTypeInternal<T>(sourceUnit, predicate, visited, reference, key);
                        if (result is T _typed && (predicate == null || predicate(_typed)))
                            return _typed;
                    }
                }
            }

            return null;
        }

        public static string GetSearchName(IGraphElement element)
        {
            if (element is Unit unit)
            {
                return ReadableName(unit);
            }
            else if (element is GraphGroup group)
            {
                return GetSearchName(group);
            }
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
            else if (element is StickyNote note)
            {
                return GetSearchName(note);
            }
#endif
            return GetElementDisplayName(element);
        }
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
        public static string GetSearchName(StickyNote note)
        {
            if (!string.IsNullOrEmpty(note.title) && !string.IsNullOrEmpty(note.body))
            {
                return note.title + "." + note.body;
            }
            else if (!string.IsNullOrEmpty(note.title))
            {
                return note.title;
            }
            else if (!string.IsNullOrEmpty(note.body))
            {
                return note.body;
            }
            return "Empty StickyNote";
        }
#endif
        public static string GetSearchName(GraphGroup group)
        {
            if (!string.IsNullOrEmpty(group.label) && !string.IsNullOrEmpty(group.comment))
            {
                return group.label + "." + group.comment;
            }
            else if (!string.IsNullOrEmpty(group.label))
            {
                return group.label;
            }
            else if (!string.IsNullOrEmpty(group.comment))
            {
                return group.comment;
            }
            return "Unnamed Graph Group";
        }

        private static readonly List<string> SearchablePorts = new List<string>()
        {
            "name", "Name", "label", "Label", "key", "Key"
        };

        public static string ReadableName(Unit unit)
        {
            if (unit == null) return string.Empty;

            if (unit is TriggerCustomEvent trigger && !trigger.name.hasValidConnection)
                return $"{GetDefaultValue(trigger.name)} [TriggerCustomEvent]";

            if (unit is CustomEvent customEvent && !customEvent.name.hasValidConnection)
                return $"{GetDefaultValue(customEvent.name)} [CustomEvent]";

            if (unit is BoltUnityEvent unityEvent && !unityEvent.name.hasValidConnection)
                return $"{GetDefaultValue(unityEvent.name)} [UnityEvent]";

            if (unit is BoltNamedAnimationEvent animationEvent && !animationEvent.name.hasValidConnection)
                return $"{GetDefaultValue(animationEvent.name)} [NamedAnimationEvent]";

            if (unit is UnifiedVariableUnit v && !v.name.hasValidConnection)
                return $"{GetDefaultValue(v.name)} [{GetElementDisplayName(unit)}: {v.kind}]";

            if (unit is Literal l)
                return $"{l.value} [{GetElementDisplayName(unit)}: {l.type?.CSharpName() ?? l.value?.GetType().CSharpName() ?? "Type Unknown"}]";

            if (!(unit is SUnit))
            {
                foreach (var portKey in SearchablePorts)
                {
                    if (unit.valueInputs.TryGetValue(portKey, out var input))
                    {
                        if (input != null && input.hasDefaultValue && !input.hasValidConnection)
                            return $"{GetDefaultValue(input)} [{GetElementDisplayName(unit).Trim()}]";
                    }
                }
            }

            return GetElementDisplayName(unit);
        }

        private static object GetDefaultValue(ValueInput input)
        {
            return input.unit.defaultValues[input.key];
        }

        public static string GetElementDisplayName(IGraphElement element)
        {
            if (element is MemberUnit m) return (m.member.targetType.As().CSharpName(false, true, false) + "." + m.member.info.Name + $" {GetActionDirection(m)}") ?? element.GetType().Name;
            if (element is UnifiedVariableUnit v) return $"{v.GetType().Name}";
            if (element is Literal) return $"Literal";
            return element.GetType().Name;
        }

        private static string GetActionDirection(MemberUnit memberUnit)
        {
            if (memberUnit is SetMember) return "(Set)";
            if (memberUnit is GetMember) return "(Get)";
            return "";
        }
    }
}
