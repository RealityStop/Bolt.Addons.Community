using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.UnitTools.FlowControl
{
    [TypeIcon(typeof(SelectUnit))]
    [UnitTitle("Combine")]
    [UnitCategory("Control")]
    public class CombineUnit : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit;

        private int connectionCount = 0;

        private int currentConnections = 0;

        private IDictionary<ControlConnection, bool> controlConnections;

        private bool addedConnectedPorts = false;

        private int lastInvokedFrame = -1;

        private bool canExit = false;

        protected override void Definition()
        {
            enter = ControlInput("enter", new Action<Flow>(Combine));

            exit = ControlOutput("exit");

            Relation(enter, exit);
        }

        private void Combine(Flow flow)
        {
            connectionCount = enter.connectedPorts.ToListPooled().Count;


            if (!addedConnectedPorts)
            {
                controlConnections = new Dictionary<ControlConnection, bool>();

                for (int i = 0; i < enter.validConnectedPorts.ToListPooled().Count; i++)
                {
                    controlConnections.Add(enter.validConnectedPorts.ToListPooled()[i].connection, false);
                }

                addedConnectedPorts = true;
            }

            foreach (ControlConnection key in controlConnections.Keys.ToListPooled())
            {
                if (lastInvokeFrame >= lastInvokedFrame)
                {
                    if (!controlConnections[key])
                    {
                        controlConnections[key] = true;
                        currentConnections++;
                    }
                }
               

            }
            
            if (currentConnections == connectionCount - 1)
            {
                Debug.Log("FINISHED");
                canExit = true;
            }

            if (canExit == true)
            {


                Flow _flow = Flow.New();
                _flow.Invoke(exit);
                _flow.Dispose();
                currentConnections = 0;
                canExit = false;
            }

        }
    }
}
