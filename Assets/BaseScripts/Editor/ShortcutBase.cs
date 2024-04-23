#if UNITY_EDITOR

using System.Reflection;

using UnityEditor;

using UnityEngine;
namespace ViitorCloud.Base.BaseScripts.Editor {
    public class ShortcutBase : UnityEditor.Editor {
        [MenuItem("Tools/ActiveToggle _`")] // `
        private static void ToggleActivationSelection() {
            try {
                GameObject go = Selection.activeGameObject;
                go.SetActive(!go.activeSelf);
            } catch (System.Exception) {
                // ignored
            }
        }

        [MenuItem("Tools/Clear Console _]")] //  ]
        private static void ClearConsole() {
            Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
            System.Type type = assembly.GetType("UnityEditor.LogEntries");
            MethodInfo method = type.GetMethod("Clear");
            if (method != null)
                method.Invoke(new object(), null);
        }

        [MenuItem("Tools/Clear PlayerPrefs _#]")] // SHIFT + ]
        private static void ClearPlayerPrefs() {
            PlayerPrefs.DeleteAll();
        }

        [MenuItem("Tools/Open Persistent DataPath")] // SHIFT + ]
        private static void OpenPersistentDataPath() {
            System.Diagnostics.Process process = new System.Diagnostics.Process();

            process.StartInfo.FileName = Application.persistentDataPath;
            process.Start();
        }

        [MenuItem("Tools/Enable Script Debugging")]
        public static void Enable() {
            EditorUtility.SetDirty(EditorUtility.InstanceIDToObject(0));
        }
    }
}
#endif