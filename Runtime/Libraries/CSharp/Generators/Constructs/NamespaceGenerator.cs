using Unity.VisualScripting.Community.Libraries.Humility;
using System.Collections.Generic;

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

        public override string Generate(int indent)
        {
            if (string.IsNullOrEmpty(@namespace)) return GenerateBefore(indent) + "\n" + GenerateBody(indent) + "\n" + GenerateAfter(indent);
            return base.Generate(indent);
        }

        protected override string GenerateAfter(int indent)
        {
            return string.Empty;
        }

        protected override string GenerateBefore(int indent)
        {
            var output = string.Empty;
            var _usings = Usings();
            usings.MergeUnique(_usings);
            for (int i = 0; i < usings.Count; i++)
            {
                if (!string.IsNullOrEmpty(usings[i])) output += "using".ConstructHighlight() + " " + usings[i] + ";" + ((i < usings.Count - 1) ? "\n" : string.Empty);
            }
            if (output.Contains("using")) output += string.IsNullOrEmpty(@namespace) ? "\n" : "\n\n";
            return (!string.IsNullOrEmpty(@namespace) ? output + "namespace ".ConstructHighlight() + @namespace : output);
        }

        protected override string GenerateBody(int indent)
        {
            var output = string.Empty;

            for (int i = 0; i < classes.Count; i++)
            {
                output += classes[i].Generate(indent) + (i < classes.Count - 1 ? "\n" : string.Empty);
            }

            output += structs.Count > 0 && classes.Count > 0 ? "\n" : string.Empty;

            for (int i = 0; i < structs.Count; i++)
            {
                output += structs[i].Generate(indent) + (i < structs.Count - 1 ? "\n" : string.Empty);
            }

            output += interfaces.Count > 0 && structs.Count > 0 ? "\n" : string.Empty;

            for (int i = 0; i < interfaces.Count; i++)
            {
                output += interfaces[i].Generate(indent) + (i < interfaces.Count - 1 ? "\n" : string.Empty);
            }

            output += enums.Count > 0 && interfaces.Count > 0 ? "\n" : string.Empty;

            for (int i = 0; i < enums.Count; i++)
            {
                output += enums[i].Generate(indent) + (i < enums.Count - 1 ? "\n" : string.Empty);
            }

            return output;
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