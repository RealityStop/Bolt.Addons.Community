using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.Database
{
    [UnitOrder(1)]
    [UnitTitle("Create Database")]
    [UnitCategory("Collections/Databases")]
    public class DatabaseUnit : DatabaseBaseUnit
    {
        [SerializeAs("columns")]
        private int _columns = 1;
        [Inspectable]
        [UnitHeaderInspectable("Columns")]
        public int columns
        {
            get
            {
                _columns = Mathf.Clamp(_columns, 1, 1000000);
                return _columns;
            }
            set { _columns = Mathf.Clamp(value, 1, 1000000); }
        }

        [SerializeAs("rows")]
        private int _rows = 1;
        [Inspectable]
        [UnitHeaderInspectable("Rows")]
        public int rows
        {
            get
            {
                _rows = Mathf.Clamp(_rows, 1, 1000000);
                return _rows;
            }
            set { _rows = Mathf.Clamp(value, 1, 1000000); }
        }

        [SerializeAs("depth")]
        private int _depth = 1;
        [Inspectable]
        [UnitHeaderInspectable("Depth")]
        public int depth
        {
            get
            {
                _depth = Mathf.Clamp(_depth, 1, 1000000);
                return _depth;
            }
            set { _depth = Mathf.Clamp(value, 1, 1000000); }
        }

        [DoNotSerialize]
        public ValueOutput databaseOut;

        protected override void Definition()
        {
            Func<Recursion, Database> database = newDatabase => GetNewDatabase();
            databaseOut = ValueOutput<Database>("database", database);
        }

        private Database GetNewDatabase()
        {
            Database newDatabase = new Database();
            newDatabase.cells = new Dictionary<Vector3, object>();
            
            newDatabase.columns = columns;
            newDatabase.depth = depth;
            newDatabase.rows = rows;
            

            for (int h = 1; h <= depth; h++)
            {
                for (int i = 1; i <= columns; i++)
                {
                    for (int j = 1; j <= rows; j++)
                    {
                        newDatabase.cells.Add(new Vector3(i, j, h), null);
                    }
                }
            }

            return newDatabase;
        }
}

}
