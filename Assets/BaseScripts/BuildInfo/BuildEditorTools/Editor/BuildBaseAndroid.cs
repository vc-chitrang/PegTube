#if UNITY_EDITOR
using System;
using System.IO;

using UnityEditor;
using UnityEditor.Build;

using UnityEngine;

using static Modules.Utility.Utility;

namespace ViitorCloud.Base.BaseScripts.BuildInfo.BuildEditorTools.Editor {
    public static class BuildBaseAndroid {
        // ----------------------------------------------------------------------
        // Const
        // ----------------------------------------------------------------------
        private static AndroidSdkVersions MinAndroidSDK { get { return AndroidSdkVersions.AndroidApiLevel28; } }

        public static string AndroidPluginsDir { get { return EscapePath(Path.GetFullPath(Path.Combine(Application.dataPath, "Plugins/Android"))); } }

        public static void UpdateBuildNumber() {
            string version = PlayerSettings.bundleVersion;
            if (string.IsNullOrEmpty(version)) {
                LogError("[BuildBase] Cannot update version number from empty string");
                return;
            }

            string[] splitVersion = version.Split(new[] {
                '.'
            }, StringSplitOptions.RemoveEmptyEntries);
            if (splitVersion == null || splitVersion.Length < 3) {
                string s = $"[BuildBase] Could not find a.b.c version format in: {version} (too few, {(splitVersion.Length.ToString())} parts found)";
                LogError(s);
                throw new BuildFailedException(s);
            }

            if (splitVersion.Length > 3) {
                string s = $"[BuildBase] Unexpected extra separators in version a.b.c: {version} (too many, {splitVersion.Length} parts found)";
                LogError(s);
                throw new BuildFailedException(s);
            }

            string major = splitVersion[0];
            string minor = splitVersion[1];
            string patch = splitVersion[2];

            // default to iOS build number, but use UCB build number if that is available
            string build = BuildBase.GetBuildNumber();
            Log(string.Format("[BuildBase] Constructing version from: {0}.{1}.{2} ({3})", major, minor, patch, build));

            if (!int.TryParse(major, out int majorInt) || majorInt < 0) {
                LogError("[BuildBase] Could not parse major version ({major}) from {version}");
                return;
            }

            if (!int.TryParse(minor, out int minorInt) || minorInt < 0) {
                LogError("[BuildBase] Could not parse minor version ({major}) from {version}");
                return;
            }

            if (!int.TryParse(patch, out int patchInt) || patchInt < 0) {
                LogError("[BuildBase] Could not parse patch version ({patch}) from {version}");
                return;
            }

            if (!int.TryParse(build, out int buildInt) || buildInt < 0) {
                LogError("[BuildBase] Could not parse build version ({build}) from {build}");
                return;
            }

            if (majorInt > 99) {
                LogError("[BuildBase] Major version {majorInt} is larger than 99, this will mess up the bundle version code");
                return;
            }

            if (minorInt > 99) {
                LogError("[BuildBase] Minor version {majorInt} is larger than 99, this will mess up the bundle version code");
                return;
            }

            if (patchInt > 99) {
                LogError("[BuildBase] Patch version {majorInt} is larger than 99, this will mess up the bundle version code");
                return;
            }

            if (buildInt > 999) {
                LogError("[BuildBase] Build version {buildInt} is larger than 999, this will mess up the bundle version code");
                return;
            }

            long bundleVersionCode = (10000000 * majorInt) + (100000 * minorInt) + (1000 * patchInt) + (1 * buildInt);
            Log(string.Format("[BuildBase] Set bundleVersionCode: {0} => {1}", PlayerSettings.Android.bundleVersionCode, bundleVersionCode));

            PlayerSettings.Android.bundleVersionCode = (int)bundleVersionCode;
        }

        private static string EscapePath(string path) {
            return $"\"{path}\"";
        }

        // TODO: Support not building shared and vision AARs, just using the ones that exist
        public static void PreBuildPlayer() {
            Log(string.Format("[Build] PreBuildPlayer ({0})", EditorUserBuildSettings.selectedBuildTargetGroup));

            PlayerSettings.Android.minSdkVersion = MinAndroidSDK;
            EditorUserBuildSettings.androidETC2Fallback = AndroidETC2Fallback.Quality32BitDownscaled;

#if ANDROID_MONO
			Log("[Build] ANDROID_MONO Set, forcing scripting backend");
			PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
#elif ANDROID_IL2CPP
			Log("[Build] ANDROID_IL2CPP Set, forcing scripting backend");
			PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
#elif RELEASE
            Log("[Build] Release build, using IL2CPP backend for runtime performance");
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
#else
			Log("[Build] Non-Release build, using Mono backend for build speed");
			PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
#endif
            Log(string.Format("[Build] Current scripting implementation: {0}", PlayerSettings.GetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup)));

            SafeAssetDatabase.Refresh();
        }

        // public static void InstallAPK(string path) {
        //     string[] installArgs = {
        //         path[(path.LastIndexOf("/", StringComparison.Ordinal) + 1)..]
        //     };
        //     //if (!ShellScript.Execute(ViitorCloudCustomData.SharedPath + "/Editor/ViitorCloud/android~/install.sh", installArgs)) {
        //     LogError("Shell script that installs apk to device failed, see console for further details!");
        // }
    }
}

#endif