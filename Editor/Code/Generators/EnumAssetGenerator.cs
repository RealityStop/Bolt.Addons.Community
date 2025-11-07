using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;

namespace Unity.VisualScripting.Community.CSharp
{
    [CodeGenerator(typeof(EnumAsset))]
    [Serializable]
    public sealed class EnumAssetGenerator : CodeGenerator<EnumAsset>
    {
        private List<int> indices = new List<int>();

        public override ControlGenerationData CreateGenerationData()
        {
            return new ControlGenerationData(typeof(Enum), null);
        }

        public override string Generate(int indent)
        {
            if (Data != null)
            {
                var output = string.Empty;
                NamespaceGenerator @namespace = null;
                if (string.IsNullOrEmpty(Data.title)) return output;

                if (!string.IsNullOrEmpty(Data.category))
                {
                    @namespace = NamespaceGenerator.Namespace(Data.category);
                }

                EnumGenerator @enum = EnumGenerator.Enum(Data.title.LegalMemberName());
                indices.Clear();

                for (int i = 0; i < Data.items.Count; i++)
                {
                    if (!string.IsNullOrEmpty(Data.items[i].name))
                    {
                        while (indices.Contains(Data.items[i].index))
                        {
                            Data.items[i].index++;
                        }

                        indices.Add(Data.items[i].index);

                        @enum.AddItem(Data.items[i].name, Data.items[i].index);
                    }
                }

                @enum.indexing = Data.useIndex;

                foreach (var name in Data.lastCompiledNames)
                {
                    if (!string.IsNullOrEmpty(Data.GetFullTypeName()) && name != Data.GetFullTypeName())
                    {
                        if (!string.IsNullOrEmpty(name))
                            @enum.AddAttribute(AttributeGenerator.Attribute<RenamedFromAttribute>().AddParameter(name));
                    }
                }

                if (@namespace != null)
                {
                    @namespace.AddEnum(@enum);
                    return @namespace.Generate(indent);
                }

                return @enum.Generate(indent);
            }

            return string.Empty;
        }
    }
}
