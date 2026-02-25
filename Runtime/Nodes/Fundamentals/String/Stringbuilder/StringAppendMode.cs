namespace Unity.VisualScripting.Community 
{
    [RenamedFrom("Unity.VisualScripting.Community.AppendStringType")]
    public class StringAppendMode
    {
        public StringBuilderUnit.AppendMode appendMode;
        public string delimiter;      // Used for 'Delimiter' mode.
        public string prefix;         // Used for 'Prefixed' mode.
        public string suffix;         // Used for 'Suffixed' mode.
        public int repeatCount = 1;   // Used for 'Repeated' mode (default to 1).
    } 
}