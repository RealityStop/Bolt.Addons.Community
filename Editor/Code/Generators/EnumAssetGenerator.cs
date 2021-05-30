﻿using Bolt.Addons.Integrations.Continuum.CSharp;
using System.Collections.Generic;

namespace Bolt.Addons.Community.Code.Editor
{
    [CodeGenerator(typeof(EnumAsset))]
    public sealed class EnumAssetGenerator : CodeGenerator<EnumAsset>
    {
        private List<int> indices = new List<int>();

        public override string Generate(int indent)
        {
            var output = string.Empty;
            NamespaceGenerator @namespace = null;
            EnumGenerator @enum = null;

            if (string.IsNullOrEmpty(Data.title)) return output;

            if (!string.IsNullOrEmpty(Data.category))
            {
                @namespace = NamespaceGenerator.Namespace(Data.category);
            }

            @enum = EnumGenerator.Enum(Data.title.LegalMemberName());

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

            if (@namespace != null)
            {
                @namespace.AddEnum(@enum);
                return @namespace.Generate(indent);
            }

            return @enum.Generate(indent);
        }
    }
}