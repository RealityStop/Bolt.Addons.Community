using Bolt.Addons.Community.Fundamentals;
using Unity.VisualScripting;
using UnityEditor;

namespace Bolt.Addons.Community.Utility.Editor
{
    [UnitCategory("Community/Editor/Windows")]
    [TypeIcon(typeof(EditorWindow))]
    public abstract class EditorWindowVariableUnit : ProxyEditorUnit
    {
        [Serialize]
        [Inspectable]
        [UnitHeaderInspectable]
        public EditorWindowAsset asset;
        [Serialize]
        public string defaultName = string.Empty;
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput name;
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput target;

        protected override void Definition()
        {
            target = ValueInput<EditorWindowView>("target");
            name = ValueInput<string>("name", defaultName);
        }
    }
}