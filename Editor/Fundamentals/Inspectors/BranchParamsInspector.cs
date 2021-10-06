//
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace Unity.VisualScripting.Community.Inspectors
//{
//    [Inspector(typeof(BranchParams))]
//    public class BranchParamsInspector : ReflectedInspector
//    {
//        public BranchParamsInspector(Metadata metadata) : base(metadata)
//        {
//        }
//        protected override void OnMemberGUI(Metadata member, Rect memberPosition)
//        {
//            Debug.Log(member.path);
//            foreach (var item in member.Keys)
//            {
//                Debug.Log("--" + item);
//            }

//            if (member.path.Equals("Root.FlowGraph.elements.BranchParams.AllowEquals", StringComparison.CurrentCultureIgnoreCase))
//            {
//                //Debug.Log("a");
//                //Debug.Log(member.parent["SupportsEqual"]);
//                //Debug.Log("a2");

//                //var val = member.parent.Member("Root.FlowGraph.elements.BranchParams.supportsEqual");

//                //Debug.Log("b");
//                //if (val != null)
//                //{

//                //    Debug.Log("c");
//                //    Debug.Log(val.path);
//                //}


//                //Debug.Log("d");
//                //Debug.Log(member.parent["Root.FlowGraph.elements.BranchParams.supportsEqual"].value);

//                //if (!((bool)member.parent["Root.FlowGraph.elements.BranchParams.supportsEqual"].value))
//                //    return;
//            }

//            base.OnMemberGUI(member, memberPosition);
//        }
//    }
//    */
//}
