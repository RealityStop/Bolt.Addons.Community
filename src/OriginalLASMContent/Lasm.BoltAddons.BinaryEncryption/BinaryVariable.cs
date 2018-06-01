using System;
using System.Collections.Generic;
using Ludiq;
using Bolt;

namespace Lasm.BoltAddons.BinaryEncryption
{
    [Serializable][Inspectable][UnitShortTitle("Binary Variable")]
    public class BinaryVariable : System.Object
    {
        [Inspectable]
        public string name;
        [Inspectable]
        public object variable;
    }
  
}
