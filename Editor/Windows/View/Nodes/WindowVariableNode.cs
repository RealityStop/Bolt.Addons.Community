using UnityEditor;

namespace Unity.VisualScripting.Community
{
    [UnitCategory("Community/Editor")]
    [TypeIcon(typeof(EditorWindow))]
    [RenamedFrom("Bolt.Addons.Community.Utility.Editor.EditorWindowVariableUnit")]
    public abstract class WindowVariableNode : Unit
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