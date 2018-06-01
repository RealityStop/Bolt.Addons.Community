using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.Database
{
    [UnitOrder(8)]
    [UnitSurtitle("Database")]
    [UnitShortTitle("Set Cell")]
    [UnitTitle("Set Database Cell")]
    [UnitCategory("Collections/Databases")]
    public class SetDatabaseCellUnit : DatabaseBaseUnit
    {
        [DoNotSerialize]
        public ControlInput enter;
        [DoNotSerialize]
        public ControlOutput exit;
        [DoNotSerialize]
        public ValueInput databaseIn;
        [DoNotSerialize]
        public ValueInput row;
        [DoNotSerialize]
        public ValueInput column;
        [DoNotSerialize]
        public ValueInput depth;
        [DoNotSerialize]
        [AllowsNull]
        public ValueInput value;
        [DoNotSerialize]
        public ValueOutput databaseOut;

        private Database newDatabase;

        protected override void Definition()
        {
            enter = ControlInput("enter", new Action<Flow>(Enter));
            databaseIn = ValueInput<Database>("database");
            column = ValueInput<int>("column", 1);
            row = ValueInput<int>("row", 1);
            depth = ValueInput<int>("depth", 1);
            value = ValueInput<object>("value").AllowsNull();
            exit = ControlOutput("exit");
            Func<Recursion, Database> database = newDatabase => GetNewDatabase();
            databaseOut = ValueOutput<Database>("databaseOut", database);

            Relation(enter, exit);
            Relation(enter, databaseOut);
            Relation(databaseIn, enter);
            Relation(row, enter);
            Relation(column, enter);
            Relation(depth, enter);
            Relation(value, enter);
        }
        
        private Database GetNewDatabase()
        {
            return newDatabase;
        }

        private void Enter(Flow flow)
        {
            newDatabase = databaseIn.GetValue<Database>();
            if (newDatabase.cells.Keys.Contains(new Vector3(column.GetValue<int>(), row.GetValue<int>(), depth.GetValue<int>()))) {
                newDatabase.cells[new Vector3(column.GetValue<int>(), row.GetValue<int>(), depth.GetValue<int>())] = value.GetValue<object>();
                Flow _flow = Flow.New();
                _flow.Invoke(exit);
                _flow.Dispose();
            }
            else
            {
                if (row.GetValue<int>() <= 0 || column.GetValue<int>() <= 0 || depth.GetValue<int>() <= 0)
                {
                    throw new NullReferenceException("You can't have a 0 row, 0 column, or 0 depth database.");
                }
                else
                {
                    throw new NullReferenceException("Cell in column '" + column.ToString() + "', row '" + row.ToString() + "', depth '" + depth.ToString() + "' does not exist.You do not have enough rows or columns.");
                }
            }
        }
    }

}
