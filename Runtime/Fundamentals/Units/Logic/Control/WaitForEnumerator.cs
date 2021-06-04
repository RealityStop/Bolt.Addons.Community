using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals.Units.logic
{
    [UnitCategory("Community/Control/Time")]
    public class WaitForEnumerator : WaitUnit
    {
        [DoNotSerialize]
        public ValueInput enumerator;

        protected override void Definition()
        {
            base.Definition();

            enumerator = ValueInput<IEnumerator>("enumerator");
        }
        protected override IEnumerator Await(Flow flow)
        {
            var _enumerator = flow.GetValue<IEnumerator>(enumerator);
            while (_enumerator.MoveNext()) yield return _enumerator.Current;
            yield return exit;
        }
    }
}