using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.Database
{
    [UnitOrder(4)]
    [UnitTitle("Add Database Depth")]
    [UnitCategory("Collections/Databases")]
    public class AddDepthUnit : AddDatabaseBaseUnit
    {
        [DoNotSerialize]
        public ControlInput enter;
        [DoNotSerialize]
        public ControlOutput exit;
        [DoNotSerialize]
        public ValueInput databaseIn;
        [DoNotSerialize]
        public ValueInput depth;
        [DoNotSerialize]
        public ValueInput index;
        [DoNotSerialize]
        public ValueOutput databaseOut;
        public Database newDatabase = new Database();

        protected override void Definition()
        {
            enter = ControlInput("enter", new Action<Flow>(Enter));
            databaseIn = ValueInput<Database>("database");
            depth = ValueInput<int>("depth", 1);
            index = ValueInput<int>("index", 1);
            Func<Recursion, Database> database = newDatabase => GetNewDatabase();
            databaseOut = ValueOutput<Database>("databaseOut", database);
            exit = ControlOutput("exit");

            Relation(enter, exit);
            Relation(enter, databaseOut);
            Relation(databaseIn, enter);
            Relation(depth, enter);
            Relation(index, enter);
        }

        public void Enter(Flow flow)
        {
            IDictionary<Vector3, object> newDictionary = new Dictionary<Vector3, object>();

            bool after = true;

            foreach (KeyValuePair<Vector3, object> item in databaseIn.GetValue<Database>().cells)
            {
                if (item.Key.z < index.GetValue<int>())
                {
                    newDictionary.Add(item.Key, item.Value);
                }
                else
                {
                    if (item.Key.z == databaseIn.GetValue<Database>().depth)
                    {
                        after = false;
                    }
                    break;
                }
            }

            for (int h = 1; h <= databaseIn.GetValue<Database>().columns; h++)
            {
                for (int i = index.GetValue<int>(); i <= index.GetValue<int>() + depth.GetValue<int>(); i++)
                {
                    for (int j = 1; j <= databaseIn.GetValue<Database>().rows; j++)
                    {
                        newDictionary.Add(new Vector3(h, j, i), null);
                    }
                }
            }

            if (after)
            {
                foreach (KeyValuePair<Vector3, object> item in databaseIn.GetValue<Database>().cells)
                {
                    if (item.Key.z >= index.GetValue<int>())
                    {
                        newDictionary.Add(new Vector3(item.Key.x, item.Key.y, item.Key.z + depth.GetValue<int>()), item.Value);
                    }
                }
            }
         
            newDatabase.cells = newDictionary;
            newDatabase.rows = databaseIn.GetValue<Database>().rows;
            newDatabase.columns = databaseIn.GetValue<Database>().columns;
            newDatabase.depth = databaseIn.GetValue<Database>().depth + depth.GetValue<int>();

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
