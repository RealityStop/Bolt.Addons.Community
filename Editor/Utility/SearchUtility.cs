using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Unity.VisualScripting.Community.NodeFinderWindow;

namespace Unity.VisualScripting.Community
{
    public static class SearchUtility
    {
        public static bool IsAdvanced(string query)
        {
            if (query.Contains("|")) return true;
            if (query.Contains(">")) return true;
            return false;
        }

        public static bool SearchMatches(string query, string haystack, SearchMode searchMode, Unit unit = null)
        {
            if (!IsAdvanced(query)) return NormalSearch(query, haystack, searchMode);

            return AdvancedSearch(query, haystack, searchMode, unit, out _);
        }

        private static bool NormalSearch(string query, string haystack, SearchMode searchMode)
        {
            if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(haystack))
                return false;

            var normalizedQuery = Normalize(query);

            if (normalizedQuery == "*")
                return true;

            string normalizedHaystack = Normalize(haystack);

            return searchMode switch
            {
                SearchMode.Contains => normalizedHaystack.IndexOf(normalizedQuery, StringComparison.OrdinalIgnoreCase) >= 0,
                SearchMode.StartsWith => normalizedHaystack.StartsWith(normalizedQuery, StringComparison.OrdinalIgnoreCase),
                // SearchMode.Exact => normalizedHaystack.Equals(query, StringComparison.OrdinalIgnoreCase),
                _ => throw new UnexpectedEnumValueException<SearchMode>(searchMode)
            };
        }

        public static bool AdvancedSearch(string query, string haystack, SearchMode searchMode, Unit unit, out List<IUnitPort> ports)
        {
            if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(haystack))
            {
                ports = null;
                return false;
            }
            var normalizedHaystack = Normalize(haystack);

            var groups = query.Split('|', StringSplitOptions.RemoveEmptyEntries);

            foreach (var group in groups)
            {
                if (!IsAdvanced(group))
                {
                    if (NormalSearch(group, normalizedHaystack, searchMode))
                    {
                        ports = null;
                        return true;
                    }
                    continue;
                }

                var parts = group.Split('>', 2, StringSplitOptions.RemoveEmptyEntries);

                string unitQuery = Normalize(parts[0].Trim());
                string portQuery = parts.Length > 1 ? Normalize(parts[1].Trim()) : null;

                string portTag = null;
                if (unitQuery.EndsWith("@CI", StringComparison.OrdinalIgnoreCase)) { portTag = "CI"; unitQuery = unitQuery[..^3]; }
                else if (unitQuery.EndsWith("@CO", StringComparison.OrdinalIgnoreCase)) { portTag = "CO"; unitQuery = unitQuery[..^3]; }
                else if (unitQuery.EndsWith("@VI", StringComparison.OrdinalIgnoreCase)) { portTag = "VI"; unitQuery = unitQuery[..^3]; }
                else if (unitQuery.EndsWith("@VO", StringComparison.OrdinalIgnoreCase)) { portTag = "VO"; unitQuery = unitQuery[..^3]; }
                else if (unitQuery.EndsWith("@I", StringComparison.OrdinalIgnoreCase)) { portTag = "I"; unitQuery = unitQuery[..^2]; }
                else if (unitQuery.EndsWith("@O", StringComparison.OrdinalIgnoreCase)) { portTag = "O"; unitQuery = unitQuery[..^2]; }

                bool unitMatch = unit is not null && (unitQuery == "*" || searchMode switch
                {
                    SearchMode.Contains => normalizedHaystack.IndexOf(unitQuery, StringComparison.OrdinalIgnoreCase) >= 0,
                    SearchMode.StartsWith => normalizedHaystack.StartsWith(unitQuery, StringComparison.OrdinalIgnoreCase),
                    // SearchMode.Exact => normalizedHaystack.Equals(unitQuery, StringComparison.OrdinalIgnoreCase),
                    _ => throw new UnexpectedEnumValueException<SearchMode>(searchMode)
                });

                if (!unitMatch) continue;
                List<IUnitPort> _ports = null;
                if (portQuery != null && unit != null)
                {
                    bool portMatch = unit.validPorts
                        .Where(p => p.hasValidConnection)
                        .Where(p => PortTagMatches(p, portTag))
                        .Any(p => PortMatches(p, portQuery, searchMode, out _ports));

                    if (!portMatch) continue;
                }
                ports = _ports;
                return true;
            }
            ports = null;
            return false;
        }

        public static bool PortTagMatches(IUnitPort port, string portTag)
        {
            if (string.IsNullOrEmpty(portTag)) return true;

            return portTag switch
            {
                "I" => port is ControlInput || port is ValueInput,
                "O" => port is ControlOutput || port is ValueOutput,
                "CI" => port is ControlInput,
                "CO" => port is ControlOutput,
                "VI" => port is ValueInput,
                "VO" => port is ValueOutput,
                _ => true
            };
        }


        public static string Normalize(string input)
        {
            return input.Replace(" ", string.Empty);
        }

        public static bool PortMatches(IUnitPort unitPort, string portQuery, SearchMode searchMode, out List<IUnitPort> matchedPorts)
        {
            matchedPorts = new List<IUnitPort>();

            if (IsAdvanced(portQuery))
            {
                if (unitPort is ControlInput controlInput)
                {
                    foreach (var connection in controlInput.connections)
                    {
                        var connectedPort = connection.source;
                        bool matches = AdvancedSearch(portQuery, GetSearchName(connectedPort.unit as Unit), searchMode, connectedPort.unit as Unit, out matchedPorts);

                        if (matches)
                        {
                            matchedPorts.Add(connectedPort);
                        }
                    }
                }
                else if (unitPort is ControlOutput controlOutput)
                {
                    var connectedPort = controlOutput.connection.destination;
                    bool matches = AdvancedSearch(portQuery, GetSearchName(connectedPort.unit as Unit), searchMode, connectedPort.unit as Unit, out matchedPorts);

                    if (matches)
                    {
                        matchedPorts.Add(connectedPort);
                    }
                }
                else if (unitPort is ValueInput valueInput)
                {
                    var connectedPort = valueInput.connection.source;
                    bool matches = AdvancedSearch(portQuery, GetSearchName(connectedPort.unit as Unit), searchMode, connectedPort.unit as Unit, out matchedPorts);

                    if (matches)
                    {
                        matchedPorts.Add(connectedPort);
                    }
                }
                else if (unitPort is ValueOutput valueOutput)
                {
                    foreach (var connection in valueOutput.connections)
                    {
                        var connectedPort = connection.destination;
                        bool matches = AdvancedSearch(portQuery, GetSearchName(connectedPort.unit as Unit), searchMode, connectedPort.unit as Unit, out matchedPorts);

                        if (matches)
                        {
                            matchedPorts.Add(connectedPort);
                        }
                    }
                }
            }
            else
            {
                if (unitPort is ControlInput controlInput)
                {
                    foreach (var connection in controlInput.connections)
                    {
                        var connectedPort = connection.source;
                        bool matches = NormalSearch(portQuery, GetSearchName(connectedPort.unit as Unit), searchMode);

                        if (matches)
                        {
                            matchedPorts.Add(connectedPort);
                        }
                    }
                }
                else if (unitPort is ControlOutput controlOutput)
                {
                    var connectedPort = controlOutput.connection.destination;
                    bool matches = NormalSearch(portQuery, GetSearchName(connectedPort.unit as Unit), searchMode);

                    if (matches)
                    {
                        matchedPorts.Add(connectedPort);
                    }
                }
                else if (unitPort is ValueInput valueInput)
                {
                    var connectedPort = valueInput.connection.source;
                    bool matches = NormalSearch(portQuery, GetSearchName(connectedPort.unit as Unit), searchMode);

                    if (matches)
                    {
                        matchedPorts.Add(connectedPort);
                    }
                }
                else if (unitPort is ValueOutput valueOutput)
                {
                    foreach (var connection in valueOutput.connections)
                    {
                        var connectedPort = connection.destination;
                        bool matches = NormalSearch(portQuery, GetSearchName(connectedPort.unit as Unit), searchMode);

                        if (matches)
                        {
                            matchedPorts.Add(connectedPort);
                        }
                    }
                }
            }

            return matchedPorts?.Count > 0;
        }

        public static string GetSearchName(Unit unit)
        {
            // Do known types first then reflection
            if (unit is TriggerCustomEvent trigger && !trigger.name.hasValidConnection) return GetDefaultValue(trigger.name) + " [TriggerCustomEvent]";
            if (unit is CustomEvent customEvent && !customEvent.name.hasValidConnection) return GetDefaultValue(customEvent.name) as string + " [CustomEvent]";
            if (unit is BoltUnityEvent unityEvent && !unityEvent.name.hasValidConnection) return GetDefaultValue(unityEvent.name) as string + " [UnityEvent]";
            if (unit is BoltNamedAnimationEvent animationEvent && !animationEvent.name.hasValidConnection) return GetDefaultValue(animationEvent.name) as string + " [NamedAnimationEvent]";
            if (unit is UnifiedVariableUnit v && !v.name.hasValidConnection) return GetDefaultValue(v.name) as string + $" [{GetElementDisplayName(unit)}: {v.kind}]";
            if (unit is Literal l) return l.value + $" [{GetElementDisplayName(unit)}: {l.type?.SelectedName(BoltCore.Configuration.humanNaming) ?? l.value?.GetType().SelectedName(BoltCore.Configuration.humanNaming) ?? "Type Unknown"}]";

            ValueInput valueInput = null;

            var field = unit.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(f => f.FieldType == typeof(ValueInput) && string.Equals(f.Name, "name", StringComparison.OrdinalIgnoreCase));

            if (field != null)
                valueInput = field.GetValue(unit) as ValueInput;

            if (valueInput == null)
            {
                var prop = unit.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(p => p.PropertyType == typeof(ValueInput) && string.Equals(p.Name, "name", StringComparison.OrdinalIgnoreCase));

                if (prop != null)
                    valueInput = prop.GetValue(unit) as ValueInput;
            }

            if (valueInput != null && valueInput.hasDefaultValue && !valueInput.hasValidConnection)
            {
                return $"{GetDefaultValue(valueInput)} [{GetElementDisplayName(unit).Trim()}]";
            }

            return GetElementDisplayName(unit);
        }

        private static object GetDefaultValue(ValueInput input)
        {
            return input.unit.defaultValues[input.key];
        }

        public static string GetElementDisplayName(IGraphElement element)
        {
            if (element is MemberUnit m) return (m.member.targetType.SelectedName(BoltCore.Configuration.humanNaming) + "." + m.member.info.SelectedName(BoltCore.Configuration.humanNaming, GetActionDirection(m))) ?? element.GetType().Name;
            if (element is UnifiedVariableUnit v) return $"{v.GetType().Name}";
            if (element is Literal) return $"Literal";
            if (element is INesterUnit nesterUnit) return GraphTraversal.GetNesterUnitName(nesterUnit);
            if (element is INesterState nesterState) return GraphTraversal.GetNesterStateName(nesterState);
            if (element is INesterStateTransition nesterStateTransition) return GraphTraversal.GetNesterStateTransitionName(nesterStateTransition);
            return element.Descriptor()?.description?.title ?? element.GetType().Name;
        }

        private static ActionDirection GetActionDirection(MemberUnit memberUnit)
        {
            if (memberUnit is SetMember) return ActionDirection.Set;
            if (memberUnit is GetMember) return ActionDirection.Get;
            return ActionDirection.Any;
        }
    }
}
