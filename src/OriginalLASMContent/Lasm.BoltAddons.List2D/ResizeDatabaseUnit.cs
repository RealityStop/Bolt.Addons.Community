using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.Database
{
    [UnitOrder(16)]
    [UnitSurtitle("Database")]
    [UnitShortTitle("Resize")]
    [UnitTitle("Resize Database")]
    [UnitCategory("Collections/Databases")]
    public class ResizeDatabaseUnit : DatabaseBaseUnit
    {
        [DoNotSerialize]
        public ValueInput databaseIn;
        [DoNotSerialize]
        public ValueOutput databaseOut;
        [DoNotSerialize]
        public ValueInput column;
        [DoNotSerialize]
        public ValueInput row;
        [DoNotSerialize]
        public ValueInput depth;

        public Database newDatabase = new Database();

        protected override void Definition()
        {
            databaseIn = ValueInput<Database>("database");
            column = ValueInput<int>("columns", 1);
            row = ValueInput<int>("rows", 1);
            depth = ValueInput<int>("depth", 1);

            Func<Recursion, Database> _databaseOut = DatabaseOut => ReturnResizedDatabase();
            databaseOut = ValueOutput<Database>("databaseOut", _databaseOut);

            Relation(databaseIn, databaseOut);
            Relation(column, databaseOut);
            Relation(row, databaseOut);
            Relation(depth, databaseOut);
        }

        public Database ReturnResizedDatabase()
        {
            IDictionary<Vector3, object> newDictionary = new Dictionary<Vector3, object>();

            Database _databaseIn = databaseIn.GetValue<Database>();

            for (int h = 1; h <= depth.GetValue<int>(); h++)
            {
                for (int i = 1; i <= column.GetValue<int>(); i++)
                {
                    for (int j = 1; j <= row.GetValue<int>(); j++)
                    {
                        Vector3 vector = new Vector3(i, j, h);
                        if (_databaseIn.cells.ContainsKey(vector))
                        {
                            newDictionary.Add(vector, _databaseIn.cells[vector]);
                        }
                        else
                        {
                            newDictionary.Add(new Vector3(i, j, h), null);
                        }
                    }
                }
            }

            newDatabase.cells = newDictionary;
            newDatabase.rows = row.GetValue<int>();
            newDatabase.columns = column.GetValue<int>();
            newDatabase.depth = depth.GetValue<int>();

            return newDatabase;
        }
    }

}
