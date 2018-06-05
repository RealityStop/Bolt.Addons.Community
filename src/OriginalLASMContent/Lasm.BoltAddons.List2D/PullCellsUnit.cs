using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.Database
{
    [UnitOrder(17)]
    [UnitSurtitle("Database")]
    [UnitShortTitle("Pull Cells")]
    [UnitTitle("Pull Database Cells")]
    [UnitCategory("Collections/Databases")]
    public class PullCellsUnit : DatabaseBaseUnit
    {
        [DoNotSerialize]
        public ValueInput databaseIn;
        [DoNotSerialize]
        public ValueInput rowStartIndex;
        [DoNotSerialize]
        public ValueInput columnStartIndex;
        [DoNotSerialize]
        public ValueInput depthStartIndex;
        [DoNotSerialize]
        public ValueInput rows;
        [DoNotSerialize]
        public ValueInput columns;
        [DoNotSerialize]
        public ValueInput depth;
        [DoNotSerialize]
        public ValueOutput A;
        [DoNotSerialize]
        public ValueOutput B;

        public Database databaseA = new Database();
        public Database databaseB = new Database();

        protected override void Definition()
        {
            databaseIn = ValueInput<Database>("database");
            columnStartIndex = ValueInput<int>("columnStartIndex", 1);
            rowStartIndex = ValueInput<int>("rowStartIndex", 1);
            depthStartIndex = ValueInput<int>("depthStartIndex", 1);
            columns = ValueInput<int>("columns", 1);
            rows = ValueInput<int>("rows", 1);
            depth = ValueInput<int>("depth", 1);

            Func<Recursion, Database> ADatabase = returnA => ReturnA();
            A = ValueOutput<Database>("a", ADatabase);

            Func<Recursion, Database> BDatabase = returnB => ReturnB();
            B = ValueOutput<Database>("b", BDatabase);

            // Relation(databaseIn, enter);
            Relation(databaseIn, A);
            Relation(databaseIn, B);
            Relation(columnStartIndex, A);
            Relation(columnStartIndex, B);
            Relation(columns, A);
            Relation(columns, B);
            Relation(rowStartIndex, A);
            Relation(rowStartIndex, B);
            Relation(rows, A);
            Relation(rows, B);
            Relation(depthStartIndex, A);
            Relation(depthStartIndex, B);
            Relation(depth, A);
            Relation(depth, B);
        }

        public Database ReturnA()
        {
            IDictionary<Vector3, object> newDictionary = new Dictionary<Vector3, object>();

            foreach (KeyValuePair<Vector3, object> item in databaseIn.GetValue<Database>().cells)
            {
                newDictionary.Add(item);

                if (item.Key.x >= columnStartIndex.GetValue<int>() && item.Key.x <= (columnStartIndex.GetValue<int>() + (columns.GetValue<int>()-1)) &&
                    item.Key.y >= rowStartIndex.GetValue<int>() && item.Key.y <= (rowStartIndex.GetValue<int>() + (rows.GetValue<int>()-1)) &&
                    item.Key.z >= depthStartIndex.GetValue<int>() && item.Key.z <= (depthStartIndex.GetValue<int>() + (depth.GetValue<int>()-1)))
                {
                    newDictionary[item.Key] = null;
                }

                
            }
            
            databaseA.cells = newDictionary;
            databaseA.rows = databaseIn.GetValue<Database>().rows;
            databaseA.columns = databaseIn.GetValue<Database>().columns;
            databaseA.depth = databaseIn.GetValue<Database>().depth;

            return databaseA;
        }

        public Database ReturnB()
        {
            IDictionary<Vector3, object> newDictionary = new Dictionary<Vector3, object>();

            foreach (KeyValuePair<Vector3, object> item in databaseIn.GetValue<Database>().cells)
            {
                if (item.Key.x >= columnStartIndex.GetValue<int>() && item.Key.x <= (columnStartIndex.GetValue<int>() + (columns.GetValue<int>()-1)) &&
                    item.Key.y >= rowStartIndex.GetValue<int>() && item.Key.y <= (rowStartIndex.GetValue<int>() + (rows.GetValue<int>()-1)) &&
                    item.Key.z >= depthStartIndex.GetValue<int>() && item.Key.z <= (depthStartIndex.GetValue<int>() + (depth.GetValue<int>()-1)))
                {
                    newDictionary.Add(item);
                }
            }

            int _rows = 0;
            int _columns = 0;
            int _depth = 0;
            int _tempRows = 0;
            int _tempColumns = 0;
            int _tempDepth = 0;

            IDictionary<Vector3, object> reDictionary = new Dictionary<Vector3, object>();

            foreach (KeyValuePair<Vector3, object> item in newDictionary)
            {

                if (_tempColumns != item.Key.x.ConvertTo<int>())
                {
                    _tempColumns = item.Key.x.ConvertTo<int>();
                    _columns++;
                }

                if (_tempRows != item.Key.y.ConvertTo<int>())
                {
                    _tempRows = item.Key.y.ConvertTo<int>();
                    _rows++;
                }

                if (_tempDepth != item.Key.z.ConvertTo<int>())
                {
                    _tempDepth = item.Key.z.ConvertTo<int>();
                    _depth++;
                }

                KeyValuePair<Vector3, object> newItem = new KeyValuePair<Vector3, object>(new Vector3(_columns, _rows, _depth), item.Value);
                reDictionary.Add(newItem);
            }

            newDictionary = reDictionary;

            databaseB.cells = newDictionary;
            databaseB.rows = rows.GetValue<int>();
            databaseB.columns = columns.GetValue<int>();
            databaseB.depth = depth.GetValue<int>();

            return databaseB;
        }

    }

}
