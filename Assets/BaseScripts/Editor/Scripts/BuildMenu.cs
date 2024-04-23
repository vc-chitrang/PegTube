#if UNITY_EDITOR

using UnityEditor;

using ViitorCloud.Base.BaseScripts.BuildInfo.BuildEditorTools.Editor;
namespace ViitorCloud.Base.BaseScripts.Editor.Scripts {
	public static class BuildMenu {
#if UNITY_IOS
		[MenuItem("ViitorCloud/Build/iOS/Release PreBuild", isValidateFunction: false, 1)]
		public static void PreBuildIOSRelease() { BuildContainer.PreBuild(ViitorCloudBuildConfigTarget.iOS, name: null, isStandalone: false, isDevelopmentBuild: false); }

		[MenuItem("ViitorCloud/Build/iOS/Release Build", isValidateFunction: false, 2)]
		public static void StartBuildIOSRelease() { BuildContainer.StartBuild(ViitorCloudBuildConfigTarget.iOS, name: null, isStandalone: false, isDevelopmentBuild: false); }

		[MenuItem("ViitorCloud/Build/iOS/Debug PreBuild", isValidateFunction: false, 101)]
		public static void PreBuildIOSDebug() { BuildContainer.PreBuild(ViitorCloudBuildConfigTarget.iOS, name: null, isStandalone: false, isDevelopmentBuild: true); }

		[MenuItem("ViitorCloud/Build/iOS/Debug Build", isValidateFunction: false, 101)]
		public static void StartBuildIOSDebug() { BuildContainer.StartBuild(ViitorCloudBuildConfigTarget.iOS, name: null, isStandalone: false, isDevelopmentBuild: true); }
#endif

#if UNITY_ANDROID
		[MenuItem("ViitorCloud/Build/Android/Release PreBuild", isValidateFunction: false, 10)]
		public static void PreBuildAndroidRelease() { BuildContainer.PreBuild(ViitorCloudBuildConfigTarget.Android, name: null, isStandalone: false, isDevelopmentBuild: false); }

		[MenuItem("ViitorCloud/Build/Android/Release Build", isValidateFunction: false, 11)]
		public static void StartBuildAndroidRelease() { BuildContainer.StartBuild(ViitorCloudBuildConfigTarget.Android, name: null, isStandalone: false, isDevelopmentBuild: false); }

		[MenuItem("ViitorCloud/Build/Android/Debug PreBuild", isValidateFunction: false, 110)]
		public static void PreBuildAndroidDebug() { BuildContainer.PreBuild(ViitorCloudBuildConfigTarget.Android, name: null, isStandalone: false, isDevelopmentBuild: true); }

		[MenuItem("ViitorCloud/Build/Android/Debug Build", isValidateFunction: false, 111)]
		public static void StartBuildAndroidDebug() { BuildContainer.StartBuild(ViitorCloudBuildConfigTarget.Android, name: null, isStandalone: false, isDevelopmentBuild: true); }
#endif

#if UNITY_STANDALONE_WIN
        [MenuItem("ViitorCloud/Build/Windows/Release PreBuild", isValidateFunction: false, 10)]
        public static void PreBuildWindowsRelease() { BuildContainer.PreBuild(ViitorCloudBuildConfigTarget.Windows, name: null, isStandalone: false, isDevelopmentBuild: false); }

        [MenuItem("ViitorCloud/Build/Windows/Release Build", isValidateFunction: false, 11)]
        public static void StartBuildWindowsRelease() { BuildContainer.StartBuild(ViitorCloudBuildConfigTarget.Windows, name: null, isStandalone: false, isDevelopmentBuild: false); }

        [MenuItem("ViitorCloud/Build/Windows/Debug PreBuild", isValidateFunction: false, 110)]
        public static void PreBuildWindowsDebug() { BuildContainer.PreBuild(ViitorCloudBuildConfigTarget.Windows, name: null, isStandalone: false, isDevelopmentBuild: true); }

        [MenuItem("ViitorCloud/Build/Windows/Debug Build", isValidateFunction: false, 111)]
        public static void StartBuildWindowsDebug() { BuildContainer.StartBuild(ViitorCloudBuildConfigTarget.Windows, name: null, isStandalone: false, isDevelopmentBuild: true); }
#endif

#if UNITY_STANDALONE_OSX
        [MenuItem("ViitorCloud/Build/Windows/Release PreBuild", isValidateFunction: false, 10)]
        public static void PreBuildOsXRelease() { BuildContainer.PreBuild(ViitorCloudBuildConfigTarget.OSX, name: null, isStandalone: false, isDevelopmentBuild: false); }

        [MenuItem("ViitorCloud/Build/Windows/Release Build", isValidateFunction: false, 11)]
        public static void StartBuildOsXRelease() { BuildContainer.StartBuild(ViitorCloudBuildConfigTarget.OSX, name: null, isStandalone: false, isDevelopmentBuild: false); }

        [MenuItem("ViitorCloud/Build/Windows/Debug PreBuild", isValidateFunction: false, 110)]
        public static void PreBuildOsXDebug() { BuildContainer.PreBuild(ViitorCloudBuildConfigTarget.OSX, name: null, isStandalone: false, isDevelopmentBuild: true); }

        [MenuItem("ViitorCloud/Build/Windows/Debug Build", isValidateFunction: false, 111)]
        public static void StartBuildOsXDebug() { BuildContainer.StartBuild(ViitorCloudBuildConfigTarget.OSX, name: null, isStandalone: false, isDevelopmentBuild: true); }
#endif

#if UNITY_IOS && UNITY_EDITOR_OSX
        [PostProcessBuild(900)]
        public static void PListModificaiton(BuildTarget target, string path) {
            if (target != BuildTarget.iOS) {
                return;
            }

            BuildBaseIOS.PListModifications(target, path);
        }
#endif
    }
}

#endif