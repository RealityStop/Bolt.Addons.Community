using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace Unity.VisualScripting.Community.Utility
{
    [RenamedFrom("Bolt.Addons.Community.Utility.Editor.EditorStateFetcher")]
    public class EditorStateFetcher : IEditorStateFetcher
    {
        public bool IsEditorPaused()
        {
#if UNITY_EDITOR
            return EditorApplication.isPaused;
#else
            return false;
#endif
        }
    }
}
