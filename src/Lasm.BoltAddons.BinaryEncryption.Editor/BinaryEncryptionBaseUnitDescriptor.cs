using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEditor;
using UnityEngine;
using Lasm.BoltAddons.BinaryEncryption;

namespace Lasm.BoltAddons.Database.Editor
{
    [Descriptor(typeof(BinaryEncryptionBaseUnit))]
    public class BinaryEncryptionBaseUnitDescriptor : UnitDescriptor<BinaryEncryptionBaseUnit>
    {
        public BinaryEncryptionBaseUnitDescriptor(BinaryEncryptionBaseUnit unit) : base(unit)
        {
        }

        protected override void Ports(UnitPortDescriptionCollection ports)
        {
            base.Ports(ports);

            for (int i = 0; i < unit.valueInputs.Count; i++)
            {
                ports["value"].isLabelVisible = false;
                ports["valueNull"].isLabelVisible = false;
                ports["name"].isLabelVisible = false;
            }

            for (int i = 0; i < unit.valueOutputs.Count; i++)
            {
                ports["variable"].isLabelVisible = false;
            }


            
        }
        
    }
}
