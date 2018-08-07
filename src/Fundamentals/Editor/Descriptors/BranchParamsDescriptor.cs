using Ludiq;
using Bolt;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace Bolt.Addons.Community.Fundamentals
{
    [Descriptor(typeof(BranchParams))]
    public class BranchParamsDescriptor : UnitDescriptor<BranchParams>
    {
        public BranchParamsDescriptor(BranchParams unit) : base(unit) { }

        protected override EditorTexture DefinedIcon()
        {
            return EditorTexture.Load(new AssemblyResourceProvider(Assembly.GetExecutingAssembly(), "Bolt.Addons.Community.Fundamentals.Editor", "Resources"), "Icons\\arrow_switch.png", CreateTextureOptions.PixelPerfect, true);
        }
    }
}