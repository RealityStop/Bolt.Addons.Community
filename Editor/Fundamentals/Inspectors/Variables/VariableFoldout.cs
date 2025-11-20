namespace Unity.VisualScripting.Community
{
    public class VariableFoldout
    {
        public string name;
        public bool isExpanded;
        public double? hoverStartTime;
        public VariableFoldout(string name, bool expanded) { this.name = name; this.isExpanded = expanded; }
    }
}