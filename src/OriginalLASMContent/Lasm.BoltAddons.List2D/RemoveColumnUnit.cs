using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.Database
{
    [UnitOrder(4)]
    [UnitTitle("Remove Database Columns")]
    [UnitCategory("Collections/Databases")]
    public class RemoveColumnUnit : RemoveDatabaseBaseUnit
    {
        [DoNotSerialize]
        public ControlInput enter;
        [DoNotSerialize]
        public ControlOutput exit;
        [DoNotSerialize]
        public ValueInput databaseIn;
        [DoNotSerialize]
        public ValueInput columns;
        [DoNotSerialize]
        public ValueInput index;
        [DoNotSerialize]
        public ValueOutput databaseOut;
        public Database newDatabase = new Database();

        protected override void Definition()
        {
            enter = ControlInput("enter", new Action<Flow>(Enter));
            databaseIn = ValueInput<Database>("database");
            columns = ValueInput<int>("columns", 1);
            index = ValueInput<int>("index", 1);
            Func<Recursion, Database> database = newDatabase => GetNewDatabase();
            databaseOut = ValueOutput<Database>("databaseOut", database);
            exit = ControlOutput("exit");

            Relation(enter, exit);
            Relation(enter, databaseOut);
            Relation(databaseIn, enter);
            Relation(columns, enter);
            Relation(index, enter);
        }

        public void Enter(Flow flow)
        {
            IDictionary<Vector3, object> newDictionary = new Dictionary<Vector3, object>();

            foreach (KeyValuePair<Vector3, object> item in databaseIn.GetValue<Database>().cells)
            {
                if (item.Key.x < index.GetValue<int>())
                {
                    newDictionary.Add(new Vector3(item.Key.x, item.Key.y, item.Key.z), item.Value);
                } else
                {
                    if (item.Key.x >= index.GetValue<int>() + columns.GetValue<int>())
                    {
                        newDictionary.Add(new Vector3(item.Key.x - columns.GetValue<int>(), item.Key.y, item.Key.z), item.Value);
                    }
                }
            }

            newDatabase.cells = newDictionary;
            newDatabase.rows = databaseIn.GetValue<Database>().rows;
            newDatabase.columns = databaseIn.GetValue<Database>().columns - columns.GetValue<int>();
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
