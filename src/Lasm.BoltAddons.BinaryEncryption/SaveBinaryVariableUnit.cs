using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.BinaryEncryption
{
    [UnitSurtitle("Binary Encryption")]
    [UnitShortTitle("Save File")]
    public class SaveBinaryVariableUnit : BinaryEncryptionBaseUnit
    {
        [DoNotSerialize]
        public ControlInput enter;
        [DoNotSerialize]
        public ControlOutput exit;
        [Inspectable]
        [UnitHeaderInspectable("Path Type"), InspectorLabel("Path Type")]
        public PathType pathType;
        [DoNotSerialize]
        public ValueInput filename;
        [DoNotSerialize]
        public ValueInput path;
        [DoNotSerialize]
        public ValueInput variables;

        protected override void Definition()
        {
            enter = ControlInput("enter", new Action<Flow>(Enter));

            filename = ValueInput<string>("filename", "filename.extension");
            
            if (pathType == PathType.Custom)
            {
                path = ValueInput<string>("path", "C:/");
            }

            variables = ValueInput<IList>("variables");

            exit = ControlOutput("exit");
        }

        private void Enter(Flow flow)
        {

        }
        
    }

    public enum PathType
    {
        Custom,
        PersistantDataPath
    }
}

