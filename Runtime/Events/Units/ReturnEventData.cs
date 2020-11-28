using Ludiq;

namespace Bolt.Addons.Community.ReturnEvents
{
    /// <summary>
    /// Wraps up the arg data we used on the event, to use with the EventReturn unit.
    /// </summary>
    [RenamedFrom("Lasm.BoltExtensions.ReturnEventData")]
    [RenamedFrom("Lasm.UAlive.ReturnEventData")]
    public class ReturnEventData
    {
        /// <summary>
        /// The arguments from the ReturnEvent.
        /// </summary>
        public ReturnEventArg args;

        public ReturnEventData(ReturnEventArg args)
        {
            this.args = args;
        }
    }
}