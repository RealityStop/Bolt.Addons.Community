
//namespace Unity.VisualScripting.Community.DefinedEvents.Editor.Widgets
//{
//    //[Widget(typeof(TargettedDefinedEvent))]
//    //public sealed class DefinedEventWidget : UnitWidget<TargettedDefinedEvent>
//    //{
//    //    public DefinedEventWidget(FlowCanvas canvas, TargettedDefinedEvent unit) : base(canvas, unit)
//    //    {
//    //        Debug.Log($"hi2");
//    //    }

//    //    protected override float GetHeaderAddonHeight(float width)
//    //    {
//    //        return EditorGUIUtility.singleLineHeight;
//    //    }

//    //    public override void BeforeFrame()
//    //    {
//    //        base.BeforeFrame();

//    //        if (GetHeaderAddonWidth() != headerAddonPosition.width ||
//    //            GetHeaderAddonHeight(headerAddonPosition.width) != headerAddonPosition.height)
//    //        {
//    //            Reposition();
//    //        }
//    //    }

//    //    protected override void DrawHeaderAddon()
//    //    {
//    //        using (LudiqGUIUtility.labelWidth.Override(75)) // For reflected inspectors / custom property drawers
//    //        using (Inspector.adaptiveWidth.Override(true))
//    //        {
//    //            EditorGUI.BeginChangeCheck();

//    //            if (unit.IsRestricted)
//    //                LudiqGUI.Inspector(metadata["restrictedEventType"], headerAddonPosition, GUIContent.none);
//    //            else
//    //                LudiqGUI.Inspector(metadata["eventType"], headerAddonPosition, GUIContent.none);

//    //            if (EditorGUI.EndChangeCheck())
//    //            {
//    //                unit.EnsureDefined();
//    //                Reposition();
//    //            }
//    //        }
//    //    }
//    //}
//}