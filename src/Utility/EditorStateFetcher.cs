using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace Bolt.Addons.Community.Utility.Editor
{
    public class EditorStateFetcher : IEditorStateFetcher
    {
        public bool IsEditorPaused()
        {
            return EditorApplication.isPaused;
        }
    }
}
