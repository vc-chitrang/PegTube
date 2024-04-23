# if UNITY_EDITOR

using UnityEditor;
using static Modules.Utility.Utility;
using UnityEngine;
namespace ViitorCloud.Base.BaseScripts.BuildInfo.BuildEditorTools.Editor {
    public static class BuildBaseWindows {

        public static void PreBuildPlayer() {
            Log(string.Format("[Build] PreBuildPlayer ({0})", EditorUserBuildSettings.selectedBuildTargetGroup));
#if RELEASE
            Log("[Build] Release build, using IL2CPP backend for runtime performance");
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
#else
			Log("[Build] Non-Release build, using Mono backend for build speed");
			PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
#endif
            Log(string.Format("[Build] Current scripting implementation: {0}", PlayerSettings.GetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup)));

            SafeAssetDatabase.Refresh();
        }

        //#if UNITY_EDITOR
        //        public static bool IsIL2CPPEnabled() {
        //            //UnityEditor.ScriptingImplementation backend = (UnityEditor.ScriptingImplementation)UnityEditor.PlayerSettings.GetPropertyInt("ScriptingBackend", UnityEditor.BuildTargetGroup.Standalone);
        //            UnityEditor.ScriptingImplementation backend = PlayerSettings.Android

        //            if (backend != UnityEditor.ScriptingImplementation.IL2CPP) {
        //                LogError("Warning: If the scripting backend is not IL2CPP there may be problems");
        //            }
        //            return backend != UnityEditor.ScriptingImplementation.IL2CPP;
        //        }
        //#endif
    }
}
#endif