using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.Database
{
    [UnitOrder(12)]
    [UnitTitle("Clear Database")]
    [UnitCategory("Collections/Databases")]
    public class ClearDatabaseUnit : DatabaseBaseUnit
    {
        [DoNotSerialize]
        public ControlInput enter;
        [DoNotSerialize]
        public ControlOutput exit;
        [DoNotSerialize]
        public ValueInput databaseIn;
        [DoNotSerialize]
        public ValueOutput databaseOut;

        public Database newDatabase = new Database();

        protected override   void Definition()
        {
            enter = ControlInput("enter", new Action<Flow>(Enter));

            databaseIn = ValueInput<Database>("database");

            Func<Recursion, Database> database = newDatabase => GetNewDatabase();
            databaseOut = ValueOutput<Database>("databaseOut", database);

            exit = ControlOutput("exit");

            Relation(enter, exit);
            Relation(enter, databaseOut);
            Relation(databaseIn, enter);
        }

        public void Enter(Flow flow)
        {
            IDictionary<Vector3, object> newDictionary = new Dictionary<Vector3, object>();

            foreach (KeyValuePair<Vector3, object> item in databaseIn.GetValue<Database>().cells)
            {
                newDictionary.Add(item.Key, null);
            }

            newDatabase.cells = newDictionary;
            newDatabase.rows = databaseIn.GetValue<Database>().rows;
            newDatabase.columns = databaseIn.GetValue<Database>().columns;
            newDatabase.depth = databaseIn.GetValue<Database>().depth;

            Flow _flow = Flow.New();
            _flow.Invoke(exit);
            _flow.Dispose();
        }

        public Database GetNewDatabase()
        {
            return newDatabase;
        }
    }

}
