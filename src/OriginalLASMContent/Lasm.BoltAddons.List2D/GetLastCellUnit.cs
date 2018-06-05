using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.Database
{
    [UnitSurtitle("Database")]
    [UnitShortTitle("Get Last Cell")]
    [UnitTitle("Get Last Database Cell")]
    [UnitCategory("Collections/Databases")]
    public class GetLastCellUnit : DatabaseBaseUnit
    {
        [DoNotSerialize][AllowsNull]
        public ValueInput databaseIn;
        [DoNotSerialize][AllowsNull]
        public ValueOutput value;
        [DoNotSerialize]
        public ValueOutput columnOut;
        [DoNotSerialize]
        public ValueOutput rowOut;
        [DoNotSerialize]
        public ValueOutput depthOut;
        protected override void Definition()
        {
            databaseIn = ValueInput<Database>("database").AllowsNull();
            Func<Recursion, int> columnIndex = getColumnIndex => GetCellColumn();
            columnOut = ValueOutput<int>("columnIndex", columnIndex);
            Func<Recursion, int> rowIndex = getRowIndex => GetCellRow();
            rowOut = ValueOutput<int>("rowIndex", rowIndex);
            Func<Recursion, int> depthIndex = getDepthIndex => GetCellDepth();
            depthOut = ValueOutput<int>("depthIndex", depthIndex);
            Func<Recursion, object> cell = getCell => GetCellValue();
            value = ValueOutput<object>("value", cell);

            Relation(databaseIn, value);
            Relation(databaseIn, rowOut);
            Relation(databaseIn, columnOut);
            Relation(databaseIn, depthOut);
        }
        
        private object GetCellValue()
        {
            IDictionary<Vector3, object> _cells = databaseIn.GetValue<Database>().cells;
            object _cell = _cells[new Vector3(databaseIn.GetValue<Database>().columns, databaseIn.GetValue<Database>().rows, databaseIn.GetValue<Database>().depth)];
            return _cell;
        }

        private int GetCellColumn()
        {
            return databaseIn.GetValue<Database>().columns;
        }

        private int GetCellRow()
        {
            return databaseIn.GetValue<Database>().rows;
        }

        private int GetCellDepth()
        {
            return databaseIn.GetValue<Database>().depth;
        }
    }

}
