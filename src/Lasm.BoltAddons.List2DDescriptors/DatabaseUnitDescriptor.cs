using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEditor;
using UnityEngine;
using Lasm.BoltAddons.Database;

namespace Lasm.BoltAddons.Database.Editor
{
    [Descriptor(typeof(DatabaseUnit))]
    public class DatabaseUnitDescriptor : UnitDescriptor<DatabaseUnit>
    {
        public DatabaseUnitDescriptor(DatabaseUnit unit) : base(unit)
        {
        }

        protected override void Ports(UnitPortDescriptionCollection ports)
        {

            for (int i = 0; i < unit.valueInputs.Count; i++)
            {
                ports["databaseOut"].isLabelVisible = false;
                //ports["playerName"].label = "Name";
                //ports["player"].label = "Player";
            }

            base.Ports(ports);
        }
        
    }
}
