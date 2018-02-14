using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.Database
{
    [UnitTitle("Merge Database")]
    [UnitCategory("Collections/Database")]
    public class MergeDatabasesUnit : Unit
    {
        [DoNotSerialize]
        public ValueInput A;
        [DoNotSerialize]
        public ValueInput B;
        [DoNotSerialize]
        public ValueOutput databaseOut;
        public Database newDatabase = new Database();

        protected override void Definition()
        {
            A = ValueInput<Database>("a");
            B = ValueInput<Database>("b");
            Func<Recursion, Database> database = newDatabase => GetNewDatabase();
            databaseOut = ValueOutput<Database>("databaseOut", database);

            Relation(A, databaseOut);
            Relation(B, databaseOut);
        }

        public void Enter(Flow flow)
        {
            IDictionary<Vector3, object> newDictionary = new Dictionary<Vector3, object>();
            int _rows = A.GetValue<Database>().rows;
            int _columns = A.GetValue<Database>().columns;
            int _depth = A.GetValue<Database>().depth;

            foreach (KeyValuePair<Vector3, object> item in A.GetValue<Database>().cells)
            {
                newDictionary.Add(item.Key, item.Value);
            }

            foreach (KeyValuePair<Vector3, object> item in B.GetValue<Database>().cells)
            {
                newDictionary.Add(new Vector3(item.Key.x + _columns, item.Key.y, item.Key.z), item.Value);
            }

            newDatabase.cells = newDictionary;
           // newDatabase.rows = A.GetValue<Database>().rows + B.GetValue<Database>().rows;
           // newDatabase.columns = A.GetValue<Database>().columns;
           // newDatabase.depth = databaseIn.GetValue<Database>().depth;
        }

        public Database GetNewDatabase()
        {
            return newDatabase;
        }
    }

}
