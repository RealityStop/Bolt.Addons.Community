using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.Database
{
    [UnitOrder(2)]
    [UnitTitle("Add Database Columns")]
    [UnitCategory("Collections/Databases")]
    public class AddColumnUnit : AddDatabaseBaseUnit
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
            bool after = false;

            foreach (KeyValuePair<Vector3, object> item in databaseIn.GetValue<Database>().cells)
            {
                if (item.Key.x < index.GetValue<int>())
                {
                    newDictionary.Add(item.Key, item.Value);
                }
                else
                {
                    if (item.Key.x == databaseIn.GetValue<Database>().columns)
                    {
                        after = false;
                    }
                    break;
                }
            }

            for (int h = 1; h <= databaseIn.GetValue<Database>().depth; h++)
            {
                for (int i = index.GetValue<int>(); i <= index.GetValue<int>() + columns.GetValue<int>(); i++)
                {
                    for (int j = 1; j <= databaseIn.GetValue<Database>().rows; j++)
                    {
                        newDictionary.Add(new Vector3(i,j,h), null);
                    }
                }
            }
            
            if (after)
            {
                foreach (KeyValuePair<Vector3, object> item in databaseIn.GetValue<Database>().cells)
                {
                    if (item.Key.x >= index.GetValue<int>())
                    {
                        newDictionary.Add(new Vector3(item.Key.x + columns.GetValue<int>(), item.Key.y, item.Key.z), item.Value);
                    }
                }
            }
         
            newDatabase.cells = newDictionary;
            newDatabase.rows = databaseIn.GetValue<Database>().rows;
            newDatabase.columns = databaseIn.GetValue<Database>().columns + columns.GetValue<int>();
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
