using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.Database
{
    [UnitOrder(11)]
    [UnitSurtitle("Database")]
    [UnitShortTitle("Contains Value")]
    [UnitTitle("Contains Database Value")]
    [UnitCategory("Collections/Databases")]
    public class ContainsValueUnit : DatabaseBaseUnit
    {
        [DoNotSerialize]
        public ValueInput databaseIn;
        [DoNotSerialize]
        public ValueInput value;
        [DoNotSerialize]
        public ValueOutput contains;

        public Database newDatabase = new Database();

        protected override void Definition()
        {
            databaseIn = ValueInput<Database>("database");
            value = ValueInput<object>("value");

            Func<Recursion, bool> doescontain = Contains => DoesContain();
            contains = ValueOutput<bool>("contains", doescontain);

            Relation(databaseIn, contains);
            Relation(value, contains);
        }

        public bool DoesContain()
        {
            bool doesContain = false;

            foreach (KeyValuePair<Vector3, object> item in databaseIn.GetValue<Database>().cells)
            {
                if (item.Value == value.GetValue<object>())
                {
                    doesContain = true;
                    break;
                }
            }

            return doesContain;
        }
    }

}
