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

        public static bool SearchMatches(string query, string haystack, SearchMode searchMode, out List<MatchNode> matches, Unit unit = null)
        {
            if (!IsAdvanced(query))
            {
                matches = new List<MatchNode>();
                return NormalSearch(query, haystack, searchMode);
            }

            return AdvancedSearch(query, haystack, searchMode, unit, out matches);
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

        private static bool AdvancedSearchRecursive(Unit unit, List<string> pathParts, int index, SearchMode searchMode, out List<MatchNode> matches)
        {
            matches = new List<MatchNode>();

            if (unit == null || index >= pathParts.Count)
                return false;

            string currentQuery = pathParts[index];
            string portTag = null;

            if (currentQuery.EndsWith("@CI", StringComparison.OrdinalIgnoreCase)) { portTag = "CI"; currentQuery = currentQuery.Substring(0, currentQuery.Length - 3); }
            else if (currentQuery.EndsWith("@CO", StringComparison.OrdinalIgnoreCase)) { portTag = "CO"; currentQuery = currentQuery.Substring(0, currentQuery.Length - 3); }
            else if (currentQuery.EndsWith("@VI", StringComparison.OrdinalIgnoreCase)) { portTag = "VI"; currentQuery = currentQuery.Substring(0, currentQuery.Length - 3); }
            else if (currentQuery.EndsWith("@VO", StringComparison.OrdinalIgnoreCase)) { portTag = "VO"; currentQuery = currentQuery.Substring(0, currentQuery.Length - 3); }
            else if (currentQuery.EndsWith("@I", StringComparison.OrdinalIgnoreCase)) { portTag = "I"; currentQuery = currentQuery.Substring(0, currentQuery.Length - 2); }
            else if (currentQuery.EndsWith("@O", StringComparison.OrdinalIgnoreCase)) { portTag = "O"; currentQuery = currentQuery.Substring(0, currentQuery.Length - 2); }

            if (!(currentQuery == "*" || NormalSearch(currentQuery, SearchUtility.GetSearchName(unit), searchMode)))
                return false;

            if (index == pathParts.Count - 1)
                return true;

            foreach (var port in unit.validPorts)
            {
                if (!port.hasValidConnection || !PortTagMatches(port, portTag))
                    continue;

                if (port is ControlInput ci)
                {
                    foreach (var conn in ci.connections)
                        if (AdvancedSearchRecursive(conn.source.unit as Unit, pathParts, index + 1, searchMode, out var childMatches))
                            matches.Add(new MatchNode { Unit = conn.source.unit as Unit, Port = conn.source, Children = childMatches });
                }
                else if (port is ControlOutput co)
                {
                    var conn = co.connection.destination;
                    if (AdvancedSearchRecursive(conn.unit as Unit, pathParts, index + 1, searchMode, out var childMatches))
                        matches.Add(new MatchNode { Unit = conn.unit as Unit, Port = conn, Children = childMatches });
                }
                else if (port is ValueInput vi)
                {
                    var conn = vi.connection.source;
                    if (AdvancedSearchRecursive(conn.unit as Unit, pathParts, index + 1, searchMode, out var childMatches))
                        matches.Add(new MatchNode { Unit = conn.unit as Unit, Port = conn, Children = childMatches });
                }
                else if (port is ValueOutput vo)
                {
                    foreach (var conn in vo.connections)
                    {
                        if (AdvancedSearchRecursive(conn.destination.unit as Unit, pathParts, index + 1, searchMode, out var childMatches))
                            matches.Add(new MatchNode { Unit = conn.destination.unit as Unit, Port = conn.destination, Children = childMatches });
                    }
                }
            }

            return matches.Count > 0;
        }

        public static bool AdvancedSearch(string query, string haystack, SearchMode searchMode, Unit unit, out List<MatchNode> ports)
        {
            ports = new List<MatchNode>();

            if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(haystack))
            {
                ports = null;
                return false;
            }

            var groups = query
                .Split('|')
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            foreach (var group in groups)
            {
                if (!IsAdvanced(group))
                {
                    if (NormalSearch(group, haystack, searchMode))
                    {
                        ports = null;
                        return true;
                    }
                    continue;
                }

                var pathParts = group
                    .Split('>')
                    .Select(p => Normalize(p.Trim()))
                    .Where(p => !string.IsNullOrEmpty(p))
                    .ToList();

                if (AdvancedSearchRecursive(unit, pathParts, 0, searchMode, out var matches) && matches.Count > 0)
                {
                    ports = matches;
                    return true;
                }
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


        private static readonly Dictionary<string, string> NormalizeCache = new Dictionary<string, string>();

        public static string Normalize(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            if (NormalizeCache.TryGetValue(input, out var normalized))
                return normalized;

            normalized = input.Replace(" ", string.Empty);
            NormalizeCache[input] = normalized;
            return normalized;
        }

        public static bool PortMatches(IUnitPort unitPort, string portQuery, SearchMode searchMode, out List<MatchNode> matches)
        {
            matches = new List<MatchNode>();
            if (IsAdvanced(portQuery))
            {
                if (unitPort is ControlInput controlInput)
                {
                    foreach (var connection in controlInput.connections)
                    {
                        var connectedPort = connection.source;
                        if (AdvancedSearch(portQuery, GetSearchName(connectedPort.unit as Unit), searchMode, connectedPort.unit as Unit, out var childNodes))
                        {
                            matches.Add(new MatchNode
                            {
                                Port = connectedPort,
                                Unit = connectedPort.unit as Unit,
                                Children = childNodes
                            });
                        }
                    }
                }
                else if (unitPort is ControlOutput controlOutput)
                {
                    var connectedPort = controlOutput.connection.destination;
                    if (AdvancedSearch(portQuery, GetSearchName(connectedPort.unit as Unit), searchMode, connectedPort.unit as Unit, out var childNodes))
                    {
                        matches.Add(new MatchNode
                        {
                            Port = connectedPort,
                            Unit = connectedPort.unit as Unit,
                            Children = childNodes
                        });
                    }
                }
                else if (unitPort is ValueInput valueInput)
                {
                    var connectedPort = valueInput.connection.source;
                    if (AdvancedSearch(portQuery, GetSearchName(connectedPort.unit as Unit), searchMode, connectedPort.unit as Unit, out var childNodes))
                    {
                        matches.Add(new MatchNode
                        {
                            Port = connectedPort,
                            Unit = connectedPort.unit as Unit,
                            Children = childNodes
                        });
                    }
                }
                else if (unitPort is ValueOutput valueOutput)
                {
                    foreach (var connection in valueOutput.connections)
                    {
                        var connectedPort = connection.destination;
                        if (AdvancedSearch(portQuery, GetSearchName(connectedPort.unit as Unit), searchMode, connectedPort.unit as Unit, out var childNodes))
                        {
                            matches.Add(new MatchNode
                            {
                                Port = connectedPort,
                                Unit = connectedPort.unit as Unit,
                                Children = childNodes
                            });
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

                        if (NormalSearch(portQuery, GetSearchName(connectedPort.unit as Unit), searchMode))
                        {
                            matches.Add(new MatchNode
                            {
                                Port = connectedPort,
                                Unit = connectedPort.unit as Unit
                            });
                        }
                    }
                }
                else if (unitPort is ControlOutput controlOutput)
                {
                    var connectedPort = controlOutput.connection.destination;
                    if (NormalSearch(portQuery, GetSearchName(connectedPort.unit as Unit), searchMode))
                    {
                        matches.Add(new MatchNode
                        {
                            Port = connectedPort,
                            Unit = connectedPort.unit as Unit
                        });
                    }
                }
                else if (unitPort is ValueInput valueInput)
                {
                    var connectedPort = valueInput.connection.source;
                    if (NormalSearch(portQuery, GetSearchName(connectedPort.unit as Unit), searchMode))
                    {
                        matches.Add(new MatchNode
                        {
                            Port = connectedPort,
                            Unit = connectedPort.unit as Unit
                        });
                    }
                }
                else if (unitPort is ValueOutput valueOutput)
                {
                    foreach (var connection in valueOutput.connections)
                    {
                        var connectedPort = connection.destination;
                        if (NormalSearch(portQuery, GetSearchName(connectedPort.unit as Unit), searchMode))
                        {
                            matches.Add(new MatchNode
                            {
                                Port = connectedPort,
                                Unit = connectedPort.unit as Unit
                            });
                        }
                    }
                }
            }

            return matches?.Count > 0;
        }

        private static readonly Dictionary<Type, MemberInfo> NameMemberCache = new Dictionary<Type, MemberInfo>();

        public static string GetSearchName(IGraphElement element)
        {
            if (element is Unit unit)
            {
                return GetSearchName(unit);
            }
            else if (element is GraphGroup group)
            {
                return GetSearchName(group);
            }
            else if (element is StickyNote note)
            {
                return GetSearchName(note);
            }
            return GetElementDisplayName(element);
        }

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

        public static string GetSearchName(Unit unit)
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
                return $"{l.value} [{GetElementDisplayName(unit)}: {l.type?.SelectedName(BoltCore.Configuration.humanNaming) ?? l.value?.GetType().SelectedName(BoltCore.Configuration.humanNaming) ?? "Type Unknown"}]";

            var type = unit.GetType();
            if (!NameMemberCache.TryGetValue(type, out var member))
            {
                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (field.FieldType == typeof(ValueInput) && string.Equals(field.Name, "name", StringComparison.OrdinalIgnoreCase))
                    {
                        member = field;
                        break;
                    }
                }

                if (member == null)
                {
                    foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if (prop.PropertyType == typeof(ValueInput) && string.Equals(prop.Name, "name", StringComparison.OrdinalIgnoreCase))
                        {
                            member = prop;
                            break;
                        }
                    }
                }

                NameMemberCache[type] = member;
            }

            if (member != null)
            {
                ValueInput valueInput = member switch
                {
                    FieldInfo fi => fi.GetValue(unit) as ValueInput,
                    PropertyInfo pi => pi.GetValue(unit) as ValueInput,
                    _ => null
                };

                if (valueInput != null && valueInput.hasDefaultValue && !valueInput.hasValidConnection)
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

        public class MatchNode
        {
            public IUnitPort Port { get; set; }
            public Unit Unit { get; set; }
            public List<MatchNode> Children { get; set; } = new List<MatchNode>();
        }
    }
}
