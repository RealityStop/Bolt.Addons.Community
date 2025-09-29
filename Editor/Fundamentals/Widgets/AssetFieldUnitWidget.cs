// using System.Collections;
// using System.Collections.Generic;
// using Unity.VisualScripting;
// using Unity.VisualScripting.Community;
// using UnityEngine;

// [Widget(typeof(AssetFieldUnit))]
// public class AssetFieldUnitWidget : UnitWidget<AssetFieldUnit>
// {
//     Recursion defined = Recursion.New(2);
//     public AssetFieldUnitWidget(FlowCanvas canvas, AssetFieldUnit unit) : base(canvas, unit)
//     {
//         unit.Define();
//     }

//     public override void DrawForeground()
//     {
//         if (defined.TryEnter(unit))
//         {
//             unit.Define();
//         }

//         base.DrawForeground();
//     }
// }
