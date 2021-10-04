using System.Collections;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [UnitCategory("Community/Time")]
    [UnitTitle("Yield")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.WaitForEnumerator")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.YieldUnit")]
    public sealed class YieldNode : WaitUnit
    {
        [UnitHeaderInspectable]
        public EnumeratorType type;

        [DoNotSerialize]
        public ValueInput enumerator, instruction, coroutine;

        protected override void Definition()
        {
            base.Definition();

            switch (type)
            {
                case EnumeratorType.YieldInstruction:
                    instruction = ValueInput<YieldInstruction>("instruction");
                    break;

                case EnumeratorType.Enumerator:
                    enumerator = ValueInput<IEnumerator>("enumerator");
                    break;

                case EnumeratorType.Coroutine:
                    coroutine = ValueInput<Coroutine>("coroutine");
                    break;
            }
        }

        protected override IEnumerator Await(Flow flow)
        {
            switch (type)
            {
                case EnumeratorType.YieldInstruction:
                    var _instruction = flow.GetValue<YieldInstruction>(instruction);
                    yield return _instruction;
                    break;

                case EnumeratorType.Enumerator:
                    var _enumerator = flow.GetValue<IEnumerator>(enumerator);
                    while (_enumerator.MoveNext())
                    {
                        yield return _enumerator.Current;
                    }
                    break;

                case EnumeratorType.Coroutine:
                    var _coroutine = flow.GetValue<Coroutine>(coroutine);
                    yield return _coroutine;
                    break;
            }

            yield return exit;
        }

        public enum EnumeratorType
        {
            YieldInstruction,
            Enumerator,
            Coroutine
        }
    }
}