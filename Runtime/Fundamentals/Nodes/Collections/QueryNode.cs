using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// A unit for using Linq querying operations in a Bolt graph.
    /// </summary>
    [UnitTitle("Query")]
    [UnitCategory("Community/Collections")]
    [TypeIcon(typeof(IEnumerable))]
    [RenamedFrom("Lasm.BoltExtensions.QueryUnit")]
    [RenamedFrom("Lasm.UAlive.QueryUnit")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.Units.Collections.QueryUnit")]
    public class QueryNode : Unit
    {
        [UnitHeaderInspectable(null)]
        [InspectorWide]
        [Inspectable]
        [Serialize]
        public QueryOperation operation;

        /// <summary>
        /// The Control Input entered when we want to begin the query
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter;

        /// <summary>
        /// An optional control flow for helping to determine the condition of the current loops item in relation to the results.
        /// </summary>
        [DoNotSerialize]
        public ControlOutput body;

        /// <summary>
        /// The Control Output for when the query is complete.
        /// </summary>
        [DoNotSerialize]
        public ControlOutput exit;

        /// <summary>
        /// The ValueInput for the collection/list we will be querying through.
        /// </summary>
        [DoNotSerialize]
        public ValueInput collection;

        /// <summary>
        /// The ValueInput for the condition to check for each item in the query.
        /// </summary>
        [DoNotSerialize]
        public ValueInput condition;

        /// <summary>
        /// The ValueInput for the condition to check for each item in the query.
        /// </summary>
        [DoNotSerialize]
        public ValueInput key;

        /// <summary>
        /// The ValueOutput for the current item of the query.
        /// </summary>
        [DoNotSerialize]
        public ValueOutput item;

        /// <summary>
        /// The ValueOutput of the resulting collection after querying.
        /// </summary>
        [DoNotSerialize]
        public ValueOutput result;

        private object current;
        private IEnumerable<object> output;
        private object single;
        private bool outCondition;
        [Obsolete]
        private string serializedOperation;
         
        protected override void Definition()
        {
            enter = ControlInput("enter", (flow) =>
            {
                PerformOperation(flow);
                return exit;
            });

            collection = ValueInput<IEnumerable<object>>("collection");

            var showItem = true;
            var showBody = true;

            switch (operation)
            {
                case QueryOperation.Any:
                    showItem = false;
                    showBody = false;
                    break;
                case QueryOperation.AnyWithCondition:
                    condition = ValueInput<bool>("condition");
                    break;
                case QueryOperation.First:
                    condition = ValueInput<bool>("condition");
                    break;
                case QueryOperation.FirstOrDefault:
                    condition = ValueInput<bool>("condition");
                    break;
                case QueryOperation.OrderBy:
                    key = ValueInput<object>("key");
                    break;
                case QueryOperation.OrderByDescending:
                    key = ValueInput<object>("key");
                    break;
                case QueryOperation.Single:
                    condition = ValueInput<bool>("condition");
                    break;
                case QueryOperation.Where:
                    condition = ValueInput<bool>("condition");
                    break;
            }

            exit = ControlOutput("exit");
            if (showBody) body = ControlOutput("body");
            if (showItem) item = ValueOutput<object>("item", (flow) => { return current; });

            switch (operation)
            {
                case QueryOperation.Any:
                    result = ValueOutput<bool>("result", (flow) => { return outCondition; });
                    break;
                case QueryOperation.AnyWithCondition:
                    result = ValueOutput<bool>("result", (flow) => { return outCondition; });
                    break;
                case QueryOperation.First:
                    result = ValueOutput<object>("result", (flow) => { return single; });
                    break;
                case QueryOperation.FirstOrDefault:
                    result = ValueOutput<object>("result", (flow) => { return single; });
                    break;
                case QueryOperation.OrderBy:
                    result = ValueOutput("result", (flow) => { return output; });
                    break;
                case QueryOperation.OrderByDescending:
                    result = ValueOutput("result", (flow) => { return output; });
                    break;
                case QueryOperation.Single:
                    result = ValueOutput<object>("result", (flow) => { return single; });
                    break;
                case QueryOperation.Where:
                    result = ValueOutput("result", (flow) => { return output; });
                    break;
            }

            Succession(enter, exit);

            if(showBody) Succession(enter, body);

            if (showItem) Assignment(enter, item);

            Requirement(collection, enter);

            switch (operation)
            {
                case QueryOperation.Any:
                    break;
                case QueryOperation.AnyWithCondition:
                    Requirement(condition, enter);
                    break;
                case QueryOperation.First:
                    Requirement(condition, enter);
                    break;
                case QueryOperation.FirstOrDefault:
                    Requirement(condition, enter);
                    break;
                case QueryOperation.OrderBy:
                    Requirement(key, enter);
                    break;
                case QueryOperation.OrderByDescending:
                    Requirement(key, enter);
                    break;
                case QueryOperation.Single:
                    Requirement(condition, enter);
                    break;
                case QueryOperation.Where:
                    Requirement(condition, enter);
                    break;
            }
        }

        private void PerformOperation(Flow flow)
        {
            Flow _flow = null;
            switch (operation)
            {
                case QueryOperation.Any:
                    outCondition = flow.GetValue<IEnumerable>(collection).Cast<object>().Any();
                    break;

                case QueryOperation.AnyWithCondition:
                    outCondition = flow.GetValue<IEnumerable>(collection).Cast<object>().Any<object>((item) =>
                    {
                        current = item;
                        _flow = Flow.New(flow.stack.AsReference());
                        _flow.Invoke(body);
                        return _flow.GetValue<bool>(condition);
                    });
                    break;

                case QueryOperation.First:
                    single = flow.GetValue<IEnumerable>(collection).Cast<object>().First<object>((item) =>
                    {
                        current = item;
                        _flow = Flow.New(flow.stack.AsReference());
                        _flow.Invoke(body);
                        return _flow.GetValue<bool>(condition);
                    });
                    break;

                case QueryOperation.FirstOrDefault:
                    single = flow.GetValue<IEnumerable>(collection).Cast<object>().FirstOrDefault<object>((item) =>
                    {
                        current = item;
                        _flow = Flow.New(flow.stack.AsReference());
                        _flow.Invoke(body);
                        return _flow.GetValue<bool>(condition);
                    });
                    break;

                case QueryOperation.OrderBy:
                    output = flow.GetValue<IEnumerable>(collection).Cast<object>().OrderBy((item) =>
                    {
                        current = item;
                        _flow = Flow.New(flow.stack.AsReference());
                        _flow.Invoke(body);
                        return _flow.GetValue<object>(key);
                    });
                    break;

                case QueryOperation.OrderByDescending:
                    output = flow.GetValue<IEnumerable>(collection).Cast<object>().OrderByDescending((item) =>
                    {
                        current = item;
                        _flow = Flow.New(flow.stack.AsReference());
                        _flow.Invoke(body);
                        return _flow.GetValue<object>(key);
                    });
                    break;

                case QueryOperation.Single:
                    single = flow.GetValue<IEnumerable>(collection).Cast<object>().Single((item) =>
                    {
                        current = item;
                        _flow = Flow.New(flow.stack.AsReference());
                        _flow.Invoke(body);
                        return _flow.GetValue<bool>(condition);
                    });
                    break;

                case QueryOperation.Where:
                    output = flow.GetValue<IEnumerable>(collection).Cast<object>().Where((item) =>
                    {
                        current = item;
                        _flow = Flow.New(flow.stack.AsReference());
                        _flow.Invoke(body);
                        return _flow.GetValue<bool>(condition);
                    });
                    break;
            }
        }
    }
}