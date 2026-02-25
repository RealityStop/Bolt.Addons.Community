namespace Unity.VisualScripting.Community
{
    public struct EditorWindowEventArgs
    {
        public EditorWindowView window;

        public EditorWindowEventArgs(EditorWindowView window)
        {
            this.window = window;
        }
    }
}