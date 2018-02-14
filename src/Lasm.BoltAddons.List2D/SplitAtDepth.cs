using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.Database
{
    [UnitOrder(15)]
    [UnitSurtitle("Database")]
    [UnitShortTitle("Split At Depth")]
    [UnitTitle("Split Database At Depth")]
    [UnitCategory("Collections/Databases")]
    public class SplitAtDepthUnit : SplitDatabaseBaseUnit
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

            int _depth = 0;
            int _tempDepth = 0;

            foreach (KeyValuePair<Vector3, object> item in databaseIn.GetValue<Database>().cells)
            {
                if (item.Key.z < index.GetValue<int>())
                {
                   ADictionary.Add(new Vector3(item.Key.x, item.Key.y, item.Key.z), item.Value);
                } else
                {
                    if (item.Key.z >= index.GetValue<int>())
                    {
                        if (_tempDepth != item.Key.z.ConvertTo<int>())
                        {
                            _tempDepth = item.Key.z.ConvertTo<int>();
                            _depth++;
                        }
                        BDictionary.Add(new Vector3(item.Key.x, item.Key.y, _depth), item.Value);
                    }
                } 
            }

            int BCount = _depth;
            int ACount = databaseIn.GetValue<Database>().depth - _depth;

            

            A.cells = ADictionary;
            A.depth = ACount;
            A.columns = databaseIn.GetValue<Database>().columns;
            A.rows = databaseIn.GetValue<Database>().rows;

            B.cells = BDictionary;
            B.depth = BCount;
            B.columns = databaseIn.GetValue<Database>().columns;
            B.rows = databaseIn.GetValue<Database>().rows;

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
