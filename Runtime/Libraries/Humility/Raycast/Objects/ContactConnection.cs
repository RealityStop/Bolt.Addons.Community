namespace Unity.VisualScripting.Community.Libraries.Humility
{
    /// <summary>
    /// A struct that holds the data of a raycast and carrys a specific type of object. If the contact fails at picking up that specific type, contacted is false.
    /// </summary>
    public struct ContactConnection<TItem>
    {
        public bool contacted => item != null;
        public TItem item;
        public float contactDistance;

        public ContactConnection(TItem item, float contactDistance = 0f)
        {
            this.item = item;
            this.contactDistance = contactDistance;
        }
    }
}