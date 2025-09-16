namespace Unity.VisualScripting.Community
{
    public class ClickableRegion
    {
        public string unitId = "";
        public string code = "";
        public int startLine;
        public int endLine;
        public int startIndex;
        public int endIndex;
        public bool isClickable = true;
        public ClickableRegion(string unitId, string code, int startLine, int endLine, int startIndex, int endIndex, bool isClickable = true)
        {
            this.unitId = unitId;
            this.code = code;
            this.startLine = startLine;
            this.endLine = endLine;
            this.startIndex = startIndex;
            this.endIndex = endIndex;
            this.isClickable = isClickable;
        }
    }
}