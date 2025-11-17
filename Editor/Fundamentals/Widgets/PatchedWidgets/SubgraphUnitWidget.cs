using System.Linq;
using UnityEditor;
using UnityEngine;
#if VISUAL_SCRIPTING_1_7
using SUnit = Unity.VisualScripting.SubgraphUnit;
#else
using SUnit = Unity.VisualScripting.SuperUnit;
#endif
namespace Unity.VisualScripting.Community
{
    public class SubgraphUnitWidget : NesterUnitWidget<SUnit>, IDragAndDropHandler
    {
        public SubgraphUnitWidget(FlowCanvas canvas, SUnit unit) : base(canvas, unit) { }

        protected override NodeColorMix baseColor
        {
            get
            {
                // TODO: Move to descriptor for optimization
                using (var recursion = Recursion.New(1))
                {
                    if (unit.nest.graph?.GetUnitsRecursive(recursion).OfType<IEventUnit>().Any() ?? false)
                    {
                        return NodeColor.Green;
                    }
                }

                return base.baseColor;
            }
        }

        public DragAndDropVisualMode dragAndDropVisualMode => DragAndDropVisualMode.Generic;

        public bool AcceptsDragAndDrop()
        {
            return DragAndDropUtility.Is<ScriptGraphAsset>() && FlowDragAndDropUtility.AcceptsScript(graph);
        }

        public void PerformDragAndDrop()
        {
            UndoUtility.RecordEditedObject("Drag & Drop Macro");
            unit.nest.source = GraphSource.Macro;
            unit.nest.macro = DragAndDropUtility.Get<ScriptGraphAsset>();
            unit.nest.embed = null;
            unit.Define();
            GUI.changed = true;
        }

        public void UpdateDragAndDrop()
        {
        }

        public void DrawDragAndDropPreview()
        {
            GraphGUI.DrawDragAndDropPreviewLabel(new Vector2(edgePosition.x, outerPosition.yMax), "Replace with: " + DragAndDropUtility.Get<ScriptGraphAsset>().name, typeof(ScriptGraphAsset).Icon());
        }

        public void ExitDragAndDrop()
        {
        }
    }
}
