namespace Unity.VisualScripting.Community
{
    public sealed class EdgeTriggerLogic
    {
        private bool? _lastEdge;
        public bool ShouldTrigger(bool currentValue)
        {
            if (!_lastEdge.HasValue || _lastEdge != currentValue)
            {
                _lastEdge = currentValue;
                return true;
            }

            return false;
        }
    }
}