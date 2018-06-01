using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using Ludiq;
using UnityEngine;

namespace Lasm.BoltAddons.Database
{
    [Serializable][Inspectable]
    public class Database : System.Object, IEnumerable<Database>
    {
        [Inspectable]
        public int rows { get; set; }
        [Inspectable]
        public int columns { get; set; }
        [Inspectable]
        public int depth { get; set; }
        [Inspectable]
        public IDictionary<Vector3, object>  cells { get; set; }

        public IEnumerator<Database> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    
}
