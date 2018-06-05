using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.Database
{
    [UnitOrder(7)]
    [UnitSurtitle("Database")]
    [UnitShortTitle("Get Cell")]
    [UnitTitle("Get Database Cell")]
    [UnitCategory("Collections/Databases")]
    public class GetDatabaseCellUnit : DatabaseBaseUnit
    {
        [DoNotSerialize][AllowsNull]
        public ValueInput databaseIn;
        [DoNotSerialize]
        public ValueInput row;
        [DoNotSerialize]
        public ValueInput column;
        [DoNotSerialize]
        public ValueInput depth;
        [DoNotSerialize][AllowsNull]
        public ValueOutput value;

        protected override void Definition()
        {
            databaseIn = ValueInput<Database>("database").AllowsNull();
            column = ValueInput<int>("column", 1);
            row = ValueInput<int>("row", 1);
            depth = ValueInput<int>("depth", 1);
            Func<Recursion, object> cell = getCell => GetCell();
            value = ValueOutput<object>("value", cell);

            Relation(databaseIn, value);
            Relation(row, value);
            Relation(column, value);
            Relation(depth, value);
        }
        
        private object GetCell()
        {
            IDictionary<Vector3, object> _cells = databaseIn.GetValue<Database>().cells;
            object _cell = _cells[new Vector3(column.GetValue<int>(), row.GetValue<int>(), depth.GetValue<int>())];
            return _cell;
        }
    }

}
