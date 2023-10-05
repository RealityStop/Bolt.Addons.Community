using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;

[Inspector(typeof(Shortcut))]
public class ShortcutInspector : Inspector
{
    public ShortcutInspector(Metadata metadata) : base(metadata)
    {
    }

    protected override float GetHeight(float width, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    protected override void OnGUI(Rect position, GUIContent label)
    {
        Shortcut shortcut = (Shortcut)metadata.value;

        if (shortcut.targetPos is SubgraphUnit)
        {
            shortcut.OpenSubgraph = GUI.Toggle(position, shortcut.OpenSubgraph, "Open Subgraph");
        }
    }
}
