using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.Database
{
    [UnitOrder(3)]
    [UnitTitle("Add Database Rows")]
    [UnitCategory("Collections/Databases")]
    public class AddRowUnit : AddDatabaseBaseUnit
    {
        [DoNotSerialize]
        public ControlInput enter;
        [DoNotSerialize]
        public ControlOutput exit;
        [DoNotSerialize]
        public ValueInput databaseIn;
        [DoNotSerialize]
        public ValueInput rows;
        [DoNotSerialize]
        public ValueInput index;
        [DoNotSerialize]
        public ValueOutput databaseOut;
        public Database newDatabase = new Database();

        protected override void Definition()
        {
            enter = ControlInput("enter", new Action<Flow>(Enter));
            databaseIn = ValueInput<Database>("database");
            rows = ValueInput<int>("rows", 1);
            index = ValueInput<int>("index", 1);
            Func<Recursion, Database> database = newDatabase => GetNewDatabase();
            databaseOut = ValueOutput<Database>("databaseOut", database);
            exit = ControlOutput("exit");

            Relation(enter, exit);
            Relation(enter, databaseOut);
            Relation(databaseIn, enter);
            Relation(rows, enter);
            Relation(index, enter);
        }

        public void Enter(Flow flow)
        {
            if (index.GetValue<int>() > databaseIn.GetValue<Database>().rows)
            {
                throw new ArgumentOutOfRangeException();
            }
            else
            {

                IDictionary<Vector3, object> newDictionary = new Dictionary<Vector3, object>();
                bool after = true;

                foreach (KeyValuePair<Vector3, object> item in databaseIn.GetValue<Database>().cells)
                {
                    if (item.Key.y < index.GetValue<int>())
                    {
                        newDictionary.Add(item.Key, item.Value);
                    }
                    else
                    {
                        if (item.Key.y == databaseIn.GetValue<Database>().rows)
                        {
                            after = false;
                        }
                        break;
                    }
                }


                for (int h = 1; h <= databaseIn.GetValue<Database>().depth; h++)
                {
                    for (int i = index.GetValue<int>(); i <= index.GetValue<int>() + rows.GetValue<int>(); i++)
                    {
                        for (int j = 1; j <= databaseIn.GetValue<Database>().columns; j++)
                        {
                            newDictionary.Add(new Vector3(j, i, h), null);
                        }
                    }
                }


                if (after)
                {
                    foreach (KeyValuePair<Vector3, object> item in databaseIn.GetValue<Database>().cells)
                    {
                        if (item.Key.y > index.GetValue<int>())
                        {
                            newDictionary.Add(new Vector3(item.Key.x, item.Key.y + rows.GetValue<int>(), item.Key.z), item.Value);
                        }
                    }
                }

                newDatabase.cells = newDictionary;
                newDatabase.rows = databaseIn.GetValue<Database>().rows + rows.GetValue<int>();
                newDatabase.columns = databaseIn.GetValue<Database>().columns;
                newDatabase.depth = databaseIn.GetValue<Database>().depth;

                Flow _flow = Flow.New();
                _flow.Invoke(exit);
                _flow.Dispose();
            }
        }

        public Database GetNewDatabase()
        {
            return newDatabase;
        }
    }

}
