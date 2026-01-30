using Unity.VisualScripting.Community.Libraries.Humility;
using System.Collections.Generic;
using Unity.VisualScripting.Community.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.NamespaceGenerator")]
    public class NamespaceGenerator : TypeGenerator
    {
        public string @namespace;
        public List<ClassGenerator> classes = new List<ClassGenerator>();
        public List<StructGenerator> structs = new List<StructGenerator>();
        public List<EnumGenerator> enums = new List<EnumGenerator>();
        public List<InterfaceGenerator> interfaces = new List<InterfaceGenerator>();
        public string beforeUsings;

        private NamespaceGenerator() { }

        public static NamespaceGenerator Namespace(string @namespace)
        {
            var namespc = new NamespaceGenerator();
            namespc.@namespace = @namespace?.Replace("/", ".").RemoveAllButLettersorDigitsExcept(new string[] { "_", "." }).RemoveIllegalFirstLetters();
            if (string.IsNullOrEmpty(namespc.@namespace)) namespc.hideBrackets = true;
            return namespc;
        }

        public void AddUsing(string @namespace)
        {
            usings.AddNonContainedTypes<string>(new List<string>() { @namespace });
        }

        public override List<string> Usings()
        {
            var usings = new List<string>();

            for (int i = 0; i < classes.Count; i++)
            {
                usings.MergeUnique(classes[i].Usings());
            }

            for (int i = 0; i < structs.Count; i++)
            {
                usings.MergeUnique(structs[i].Usings());
            }

            for (int i = 0; i < enums.Count; i++)
            {
                usings.MergeUnique(enums[i].Usings());
            }

            for (int i = 0; i < interfaces.Count; i++)
            {
                usings.MergeUnique(interfaces[i].Usings());
            }

            if (usings.Contains(@namespace)) usings.Remove(@namespace);

            usings.MergeUnique(this.usings);
            return usings;
        }

        public override void Generate(CodeWriter writer, ControlGenerationData data)
        {
            if (string.IsNullOrEmpty(@namespace))
            {
                GenerateBefore(writer, data);
                GenerateBody(writer, data);
                GenerateAfter(writer, data);
                return;
            }
            base.Generate(writer, data);
        }

        protected override void GenerateAfter(CodeWriter writer, ControlGenerationData data)
        {
        }

        protected override void GenerateBefore(CodeWriter writer, ControlGenerationData data)
        {
            if (!string.IsNullOrEmpty(beforeUsings))
            {
                writer.WriteLine(beforeUsings);
            }

            var _usings = Usings();
            usings.MergeUnique(_usings);
            bool hasUsings = false;
            for (int i = 0; i < usings.Count; i++)
            {
                if (!string.IsNullOrEmpty(usings[i]))
                {
                    hasUsings = true;
                    writer.Write("using".ConstructHighlight() + " " + usings[i] + ";");
                    if (i < usings.Count - 1)
                    {
                        writer.NewLine();
                    }
                }
            }

            if (hasUsings)
            {
                writer.NewLine();
                writer.NewLine();
            }

            if (!string.IsNullOrEmpty(@namespace))
            {
                writer.Write("namespace ".ConstructHighlight() + @namespace);
                writer.NewLine();
            }
        }

        protected override void GenerateBody(CodeWriter writer, ControlGenerationData data)
        {
            for (int i = 0; i < classes.Count; i++)
            {
                classes[i].Generate(writer, data);
                if (i < classes.Count - 1) writer.NewLine();
            }

            if (structs.Count > 0 && classes.Count > 0)
                writer.NewLine();

            for (int i = 0; i < structs.Count; i++)
            {
                structs[i].Generate(writer, data);
                if (i < structs.Count - 1) writer.NewLine();
            }

            if (interfaces.Count > 0 && structs.Count > 0)
                writer.NewLine();

            for (int i = 0; i < interfaces.Count; i++)
            {
                interfaces[i].Generate(writer, data);
                if (i < interfaces.Count - 1) writer.NewLine();
            }

            if (enums.Count > 0 && interfaces.Count > 0)
                writer.NewLine();

            for (int i = 0; i < enums.Count; i++)
            {
                enums[i].Generate(writer, data);
                if (i < enums.Count - 1) writer.NewLine();
            }

        }

        public NamespaceGenerator AddClass(ClassGenerator @class)
        {
            classes.Add(@class);
            return this;
        }

        public NamespaceGenerator AddStruct(StructGenerator @struct)
        {
            structs.Add(@struct);
            return this;
        }

        public NamespaceGenerator AddEnum(EnumGenerator @enum)
        {
            enums.Add(@enum);
            return this;
        }

        public NamespaceGenerator AddInterface(InterfaceGenerator @interface)
        {
            interfaces.Add(@interface);
            return this;
        }
    }
}