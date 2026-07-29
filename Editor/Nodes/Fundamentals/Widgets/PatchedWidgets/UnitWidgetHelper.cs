using System;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    internal class UnitWidgetHelper
    {
        internal static void ReplaceUnit(IUnit unit, GraphReference reference, IGraphContext context, GraphSelection selection, EventWrapper eventWrapper)
        {
            var oldUnit = unit;
            var unitPosition = oldUnit.position;
            var preservation = UnitPreservation.Preserve(oldUnit);

            var options = new UnitOptionTree(new GUIContent("Node"));
            options.filter = new UnitOptionFilter(true);
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
            options.filter.NoConnection = false;
#endif
            options.reference = reference;

            var activatorPosition = new Rect(eventWrapper.mousePosition, new Vector2(200, 1));

            LudiqGUI.FuzzyDropdown
            (
                activatorPosition,
                options,
                null,
                delegate (object _option)
                {
                    var option = (IUnitOption)_option;

                    context.BeginEdit();
                    UndoUtility.RecordEditedObject("Replace Node");
                    var graph = oldUnit.graph;
                    oldUnit.graph.units.Remove(oldUnit);
                    var newUnit = option.InstantiateUnit();
                    newUnit.guid = Guid.NewGuid();
                    newUnit.position = unitPosition;
                    graph.units.Add(newUnit);
                    preservation.RestoreTo(newUnit);
                    option.PreconfigureUnit(newUnit);
                    selection.Select(newUnit);
                    GUI.changed = true;
                    context.EndEdit();
                }
            );
        }
    }
}