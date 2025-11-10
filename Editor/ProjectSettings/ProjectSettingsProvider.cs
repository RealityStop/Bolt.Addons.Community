using UnityEditor;

namespace Unity.VisualScripting.Community
{
    public class ProjectSettingsProvider : Editor
    {
        [SettingsProvider]
        public static SettingsProvider CreateProjectSettingProvider()
        {
            return new ProjectSettingsProviderView();
        }
    }
}
