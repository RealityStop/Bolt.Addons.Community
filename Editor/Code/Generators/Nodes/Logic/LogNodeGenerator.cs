using System.Text;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(LogNode))]
    public sealed class LogNodeGenerator : NodeGenerator<LogNode>
    {
        public LogNodeGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            var unit = Unit;

            writer.WriteIndented();
            writer.Write(typeof(Debug)).Dot();
            
            string logMethod = unit.type switch
            {
                LogType.Warning => "LogWarning",
                LogType.Error => "LogError",
                _ => "Log"
            };
            
            writer.Write(logMethod);
            writer.Write("(");

            if (unit.argumentCount == 0)
            {
                GenerateValue(unit.format, data, writer);
            }
            else
            {
                writer.Write("string".ConstructHighlight() + ".Format(");
                GenerateValue(unit.format, data, writer);
                
                for (int i = 0; i < unit.argumentCount; i++)
                {
                    writer.Write(", ");
                    GenerateValue(unit.arguments[i], data, writer);
                }
                
                writer.Write(")");
            }

            writer.WriteEnd();
            GenerateExitControl(unit.output, data, writer);
        }
    }
}