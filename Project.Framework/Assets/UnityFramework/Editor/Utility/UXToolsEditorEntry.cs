using UnityEditor;
using UnityFramework.Editor.UXTools;

namespace UnityFramework.Editor
{
    public static class UXToolsEditorEntry
    {
        [MenuItem("UnityFramework/UXTools/Widget Library", false, 200)]
        private static void OpenWidgetLibrary()
        {
            WidgetRepositoryWindow.OpenWindow();
        }

        [MenuItem("UnityFramework/UXTools/Recent Opened Prefabs", false, 201)]
        private static void OpenRecentTemplates()
        {
            PrefabRecentWindow.OpenWindow();
        }

        [MenuItem("UnityFramework/UXTools/Recent Selected Files", false, 202)]
        private static void OpenRecentSelectedFiles()
        {
            RecentFilesWindow.ShowWindow();
        }
    }
}
