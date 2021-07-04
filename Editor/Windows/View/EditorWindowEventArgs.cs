namespace Bolt.Addons.Community.Utility.Editor
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