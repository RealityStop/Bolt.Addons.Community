using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.Database
{
    [UnitOrder(9)]
    [UnitSurtitle("Database")]
    [UnitShortTitle("Find Cell")]
    [UnitTitle("Find Database Cell")]
    [UnitCategory("Collections/Databases")]
    public class FindCellUnit : DatabaseBaseUnit
    {
        [DoNotSerialize]
        public ValueInput databaseIn;
        [DoNotSerialize]
        public ValueInput value;
        [DoNotSerialize]
        public ValueOutput column;
        [DoNotSerialize]
        public ValueOutput row;
        [DoNotSerialize]
        public ValueOutput depth;

        public Database newDatabase = new Database();

        protected override void Definition()
        {
            databaseIn = ValueInput<Database>("database");
            value = ValueInput<object>("value");

            Func<Recursion, int> columnIndex = ColumnIndex => ReturnColumnIndex();
            column = ValueOutput<int>("column", columnIndex);

            Func<Recursion, int> rowIndex = RowIndex => ReturnRowIndex();
            row = ValueOutput<int>("row", rowIndex);

            Func<Recursion, int> depthIndex = DepthIndex => ReturnDepthIndex();
            depth = ValueOutput<int>("depth", depthIndex);

            Relation(databaseIn, column);
            Relation(databaseIn, row);
            Relation(databaseIn, depth);
            Relation(value, column);
            Relation(value, row);
            Relation(value, depth);
        }

        public int ReturnColumnIndex()
        {
            int columnIndex = 0;

            foreach (KeyValuePair<Vector3, object> item in databaseIn.GetValue<Database>().cells)
            {
                if (item.Value == value.GetValue<object>())
                {
                    columnIndex = item.Key.x.ConvertTo<int>();
                    break;
                }
            }

            return columnIndex;
        }

        public int ReturnRowIndex()
        {
            int rowIndex = 0;

            foreach (KeyValuePair<Vector3, object> item in databaseIn.GetValue<Database>().cells)
            {
                if (item.Value == value.GetValue<object>())
                {
                    rowIndex = item.Key.y.ConvertTo<int>();
                    break;
                }
            }

            return rowIndex;
        }

        public int ReturnDepthIndex()
        {
            int depthIndex = 0;

            foreach (KeyValuePair<Vector3, object> item in databaseIn.GetValue<Database>().cells)
            {
                if (item.Value == value.GetValue<object>())
                {
                    depthIndex = item.Key.z.ConvertTo<int>();
                    break;
                }
            }

            return depthIndex;
        }
    }

}
