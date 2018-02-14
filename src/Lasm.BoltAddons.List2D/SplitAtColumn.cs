using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.Database
{
    [UnitOrder(13)]
    [UnitSurtitle("Database")]
    [UnitShortTitle("Split At Column")]
    [UnitTitle("Split Database At Column")]
    [UnitCategory("Collections/Databases")]
    public class SplitAtColumnUnit : SplitDatabaseBaseUnit
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

            int _columns = 0;
            int _tempColumns = 0;

            foreach (KeyValuePair<Vector3, object> item in databaseIn.GetValue<Database>().cells)
            {
                if (item.Key.x < index.GetValue<int>())
                {
                   ADictionary.Add(new Vector3(item.Key.x, item.Key.y, item.Key.z), item.Value);
                } else
                {
                    if (item.Key.x >= index.GetValue<int>())
                    {
                        if (_tempColumns != item.Key.x.ConvertTo<int>())
                        {
                            _tempColumns = item.Key.x.ConvertTo<int>();
                            _columns++;
                        }
                        BDictionary.Add(new Vector3(_columns, item.Key.y, item.Key.z), item.Value);
                    }
                } 
            }

            int BCount = _columns;
            int ACount = databaseIn.GetValue<Database>().columns - _columns;

            

            A.cells = ADictionary;
            A.columns = ACount;
            A.rows = databaseIn.GetValue<Database>().rows;
            A.depth = databaseIn.GetValue<Database>().depth;

            B.cells = BDictionary;
            B.columns = BCount;
            B.rows = databaseIn.GetValue<Database>().rows;
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
