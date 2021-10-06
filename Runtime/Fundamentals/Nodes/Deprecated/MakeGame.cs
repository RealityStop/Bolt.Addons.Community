//using Bolt.Community.Addons.Utility;
//
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Unity.VisualScripting.Community.Fundamentals.Units.Documenting
//{
//    [UnitCategory("Community\\Documentation")]
//    [UnitShortTitle("Make Game")]
//    [UnitTitle("Make Game")]
//    public class MakeGame : Unit
//    {

//        [UnitHeaderInspectable]
//        [UnitButton("TriggerButton")]
//        public UnitButton go;


//        /// <summary>
//        /// The action to execute if the condition is true.
//        /// </summary>
//        [DoNotSerialize]
//        [PortLabel("Success!")]
//        public ControlOutput success { get; private set; }



//        /// <summary>
//        /// The action to execute if the condition is true.
//        /// </summary>
//        [DoNotSerialize]
//        [PortLabel("Give Up")]
//        public ControlOutput giveUp { get; private set; }



//        /// <summary>
//        /// The action to execute if the condition is true.
//        /// </summary>
//        [DoNotSerialize]
//        [PortLabel("Why is this so hard?")]
//        public ControlOutput hard { get; private set; }


//        /// <summary>
//        /// The action to execute if the condition is true.
//        /// </summary>
//        [DoNotSerialize]
//        [PortLabel("There's a button for this?")]
//        public ControlOutput button { get; private set; }


//        /// <summary>
//        /// The action to execute if the condition is true.
//        /// </summary>
//        [DoNotSerialize]
//        [PortLabel("I can haz MMORPG?")]
//        public ControlOutput mmorpg { get; private set; }

//        protected override void Definition()
//        {
//            success = ControlOutput(nameof(success));
//            giveUp = ControlOutput(nameof(giveUp));
//            hard = ControlOutput(nameof(hard));
//            button = ControlOutput(nameof(button));
//            mmorpg = ControlOutput(nameof(mmorpg));
//        }
//    }
//}