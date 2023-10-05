using System;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [CodeGenerator(typeof(ScriptGraphAsset))]
    public sealed class ScriptGraphAssetGenerator : CodeGenerator<ScriptGraphAsset>
    {
        private readonly Dictionary<string, string> EventsNames = new Dictionary<string, string>()
        {
            { "On Start", "Start" },
            { "On Update", "Update" },
            { "On Awake", "Awake" },
            { "On Fixed Update", "FixedUpdate" },
            { "On Late Update", "LateUpdate" },
          //{ "On Enable", "Enable" },
        };

        public override string Generate(int indent)
        {
            var script = string.Empty;

            if (Data?.graph == null)
            {
                return "Open the selected Graph.".TypeHighlight();
            }

            var usings = new List<string>
            {
                "Unity",
                "UnityEngine",
                "Unity.VisualScripting"
            };

            foreach (Unit unit in Data.graph.units)
            {
                if (!usings.Contains(NodeGenerator.GetSingleDecorator(unit, unit).NameSpace) && NodeGenerator.GetSingleDecorator(unit, unit).NameSpace != string.Empty)
                {
                    usings.Add(NodeGenerator.GetSingleDecorator(unit, unit).NameSpace);
                    NodeGenerator.GetSingleDecorator(unit, unit).hasNamespace = true;
                }    
            }

            var scriptGraphDecorator = ScriptGraphDecorator.GetSingleDecorator(Data);

            if (scriptGraphDecorator.usings.Count > 0)
            {
                foreach (string _using in scriptGraphDecorator.usings)
                {
                    if (!usings.Contains(_using))
                    {
                        usings.Add(_using);
                    }
                }
            }

            foreach (var ns in usings)
            {
                script += $"using".ConstructHighlight() + $" {ns};\n";
            }

            script += $"\npublic class ".ConstructHighlight() + $"{Data.graph.title}".LegalMemberName() + " : MonoBehaviour\n".TypeHighlight() + "{";

            foreach (VariableDeclaration variable in Data.graph.variables) 
            {
                script += "\n    public ".ConstructHighlight() + Type.GetType(variable.typeHandle.Identification).DisplayName().TypeHighlight() + " " + variable.name + (variable.value != null ? $" = " + "" + $"{variable.value.As().Code(true, true, true, "", false)};\n" : string.Empty + ";");
            };

            foreach (Unit unit in Data.graph.units)
            {
                if (unit is Once)
                {
                    script += "\n    private ".ConstructHighlight() + "bool ".TypeHighlight() + "Once_" + NodeGenerator.GetSingleDecorator(unit, unit).UniqueID + ";\n";
                }
            }

            foreach (object unit in Data.graph.units.Where(unit => unit is IEventUnit))
            {
                if (unit is EventUnit<EmptyEventArgs> eventUnit)
                {
                    script += (eventUnit.trigger.hasValidConnection) ? GenerateEventMethod(eventUnit, script) : string.Empty;
                }
                else
                {
                    //script += ((unit as Unit).controlOutputs[0].hasValidConnection) ? GenerateEventMethod(unit as IEventUnit, script) : string.Empty;
                }
/*                script += "test" + NodeGenerator.GetSingleDecorator(unit as Unit, unit as Unit).GenerateControl((unit as Unit).controlOutputs[0].connection.destination, new(), 2);
*/
            }
            script += "}";

            return script;
        }
        //method += $"\n    public {ReturnType} ".ConstructHighlight() + $"{(EventsNames.ContainsKey(eventUnit.Descriptor().description.title) ? EventsNames[eventUnit.Descriptor().description.title] : eventUnit.Descriptor().description.title.Replace(" ", ""))}" + (eventUnit.valueOutputs.Count() == 0 ? "()" : string.Empty
        private string GenerateEventMethod(EventUnit<EmptyEventArgs> eventUnit, string script)
        {
            var ReturnType = (eventUnit.coroutine ? "IEnumerator".TypeHighlight() : "void");
            var method = string.Empty;

            var unitName = BoltFlowNameUtility.UnitTitle(eventUnit.GetType(), false, true);
            var sanitizedKey = unitName.Trim(); // Sanitize the key

            string EventName;
            if (EventsNames.ContainsKey(sanitizedKey))
            {
                EventName = EventsNames[sanitizedKey];
            }
            else
            {
                EventName = unitName.Replace(" ", "");
            }

            if (script.Contains(BoltFlowNameUtility.UnitTitle(eventUnit.GetType(), false, true))) method += "\n     /* You cannot have more than one event of the same type \n     please remove this event */";
            method += $"\n    public {ReturnType} ".ConstructHighlight() + EventName;

            if (eventUnit.valueOutputs.Count > 0)
            {
                method += "(";
                bool firstParam = true;
                foreach (var valueOutput in eventUnit.valueOutputs)
                {
                    if (!firstParam)
                    {
                        method += ", ";
                    }
                    method += $"{valueOutput.type} {valueOutput.key.ToLower()}";
                    firstParam = false;
                }
                method += ")";
            }
            else 
            {
                method += "()";
            }

            method += "\n    {\n";
            var data = new ControlGenerationData();
            foreach (var variable in Data.graph.variables) 
            {
                data.AddLocalName(variable.name);
            }
            method += " " + NodeGenerator.GetSingleDecorator(eventUnit, eventUnit).GenerateControl(eventUnit.trigger.connection.destination, data, 2);
            method += "\n    }\n";

            return method;
        }

        private string GenerateEventMethod(IEventUnit eventUnit, string script)
        {
            var ReturnType = (eventUnit.coroutine ? "IEnumerator" : "void");
            var method = string.Empty;
            var unit = eventUnit as Unit;
            if (script.Contains(unit.Descriptor().description.title)) method += "\n     /* You cannot have more than one event of the same type \n     please remove this event */";
            method += $"\n    public {ReturnType} ".ConstructHighlight() + $"{unit.Descriptor().description.title.LegalMemberName()}";

            Type Generictype = null;
            // Use reflection to inspect constructor parameters of the event unit type
            var eventUnitType = eventUnit.GetType();

            if (eventUnitType.ContainsGenericParameters)
            {
                Generictype = eventUnitType.GetGenericArguments().FirstOrDefault();
            }

            if (Generictype.ExtendedDeclaringType(true).ContainsGenericParameters)
            {
                method += "(";
                method += $"{Generictype} {Generictype.ToString().ToLower()}";
                method += ")";
            }
            else 
            {
                method += "()";
            }

            method += "\n    {\n";
            var data = new ControlGenerationData();
            foreach (var variable in Data.graph.variables)
            {
                data.AddLocalName(variable.name);
            }

            method += " " + NodeGenerator.GetSingleDecorator(eventUnit as Unit, eventUnit as Unit).GenerateControl(eventUnit.controlOutputs[0].connection.destination, data, 2);
            method += "\n    }\n";

            return method;
        }
    }
}
