using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.Database
{
    [UnitOrder(14)]
    [UnitSurtitle("Database")]
    [UnitShortTitle("Split At Row")]
    [UnitTitle("Split Database At Row")]
    [UnitCategory("Collections/Databases")]
    public class SplitAtRowUnit : SplitDatabaseBaseUnit
    {
        [DoNotSerialize]
        public ControlInput enter;
        [DoNotSerialize]
        public ControlOutput exit;
        [DoNotSerialize]
        public ValueInput databaseIn;
        [DoNotSerialize]
        public ValueInput index;
        [DoNotSerialize]
        public ValueOutput ADatabase;
        [DoNotSerialize]
        public ValueOutput BDatabase;
        public Database A = new Database();
        public Database B = new Database();


        protected override void Definition()
        {
            enter = ControlInput("enter", new Action<Flow>(Enter));
            databaseIn = ValueInput<Database>("database");
            index = ValueInput<int>("index", 1);

            Func<Recursion, Database> AnewDatabase = newADatabase => GetNewADatabase();
            ADatabase = ValueOutput<Database>("a", AnewDatabase);

            Func<Recursion, Database> BnewDatabase = newBottomDatabase => GetNewBDatabase();
            BDatabase = ValueOutput<Database>("b", BnewDatabase);

            exit = ControlOutput("exit");

            Relation(enter, exit);
            Relation(enter, ADatabase);
            Relation(enter, BDatabase);
            Relation(databaseIn, enter);
            Relation(index, enter);
        }

        public void Enter(Flow flow)
        {
            IDictionary<Vector3, object> ADictionary = new Dictionary<Vector3, object>();
            IDictionary<Vector3, object> BDictionary = new Dictionary<Vector3, object>();

            int _rows = 0;
            int _tempRows = 0;

            foreach (KeyValuePair<Vector3, object> item in databaseIn.GetValue<Database>().cells)
            {
                if (item.Key.y < index.GetValue<int>())
                {
                   ADictionary.Add(new Vector3(item.Key.x, item.Key.y, item.Key.z), item.Value);
                } else
                {
                    if (item.Key.y >= index.GetValue<int>())
                    {
                        if (_tempRows != item.Key.y.ConvertTo<int>())
                        {
                            _tempRows = item.Key.y.ConvertTo<int>();
                            _rows++;
                        }
                        BDictionary.Add(new Vector3(item.Key.x, _rows, item.Key.z), item.Value);
                    }
                } 
            }

            int BCount = _rows;
            int ACount = databaseIn.GetValue<Database>().rows - _rows;

            

            A.cells = ADictionary;
            A.rows = ACount;
            A.columns = databaseIn.GetValue<Database>().columns;
            A.depth = databaseIn.GetValue<Database>().depth;

            B.cells = BDictionary;
            B.rows = BCount;
            B.columns = databaseIn.GetValue<Database>().columns;
            B.depth = databaseIn.GetValue<Database>().depth;

            Flow _flow = Flow.New();
            _flow.Invoke(exit);
            _flow.Dispose();
        }

        public Database GetNewADatabase()
        {
            return A;
        }

        public Database GetNewBDatabase()
        {
            return B;
        }
    }

}
