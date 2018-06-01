using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.Database
{
    [UnitOrder(10)]
    [UnitSurtitle("Database")]
    [UnitShortTitle("Find Cells")]
    [UnitTitle("Find Database Cells")]
    [UnitCategory("Collections/Databases")]
    public class FindCellsUnit : DatabaseBaseUnit
    {
        [DoNotSerialize]
        public ValueInput databaseIn;
        [DoNotSerialize]
        public ValueInput value;
        [DoNotSerialize]
        public ValueOutput cells;

        public Database newDatabase = new Database();
        private IList<Vector3> _cells;

        protected override void Definition()
        {
            databaseIn = ValueInput<Database>("database");
            value = ValueInput<object>("value");

            Func<Recursion, IList<Vector3>> getCells = GetCells => ReturnCells();
            cells = ValueOutput<IList<Vector3>>("cells", getCells);

            Relation(databaseIn, cells);
            Relation(value, cells);
        }

        public IList<Vector3> ReturnCells()
        {
            IList<Vector3> vectors = new List<Vector3>();

            foreach (KeyValuePair<Vector3, object> item in databaseIn.GetValue<Database>().cells)
            {
                if (item.Value == value.GetValue<object>())
                {
                    vectors.Add(item.Key);
                }
            }

            return vectors;
        }

        
    }

}
