#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using static Modules.Utility.Utility;
using UnityEngine;
using UnityEngine.Serialization;

using ViitorCloud.Base.BaseScripts.Internal.Scripts.Models.Data.GameInfo;

namespace ViitorCloud.Base.BaseScripts.BuildInfo.BuildEditorTools.Editor {

    [Serializable]
    public class ViitorCloudBuildConfigObject {

        [FormerlySerializedAs("Name")]
        [Header("Basic Info (required)")]
        public string name;

        [FormerlySerializedAs("Target")]
        public ViitorCloudBuildConfigTarget target;
        [FormerlySerializedAs("IsDevelopmentBuild")]
        public bool isDevelopmentBuild;
        [FormerlySerializedAs("IsBetaBuild")]
        public bool isBetaBuild;
        [FormerlySerializedAs("LaunchExecutable")]
        public bool launchExecutable;

        //public List<string> Scenes = new List<string> { "Assets/main.unity" };
        [FormerlySerializedAs("Scenes")]
        public List<SceneAssets> scenes = new List<SceneAssets>();

        [FormerlySerializedAs("ProductName")]
        [Header("Overrides (optional)")]
        public string productName;

        [FormerlySerializedAs("CompanyName")]
        public string companyName;
        [FormerlySerializedAs("BundleId")]
        public string bundleId;
        [FormerlySerializedAs("BuildPath")]
        public string buildPath;
        [FormerlySerializedAs("IconPath")]
        public string iconPath;
        public string splashScreenLogo;
        [FormerlySerializedAs("splashScreenLogoBG")]
        public string splashScreenLogoBg;
        [FormerlySerializedAs("blurSplashBG")]
        public bool blurSplashBg;

        [FormerlySerializedAs("Portrait")]
        [Header("Screen Orientation (optional)")]
        public bool portrait;

        [FormerlySerializedAs("PortraitUpsideDown")]
        public bool portraitUpsideDown;
        [FormerlySerializedAs("LandscapeLeft")]
        public bool landscapeLeft;
        [FormerlySerializedAs("LandscapeRight")]
        public bool landscapeRight;

        [FormerlySerializedAs("CompileDefineAdditions")]
        public List<string> compileDefineAdditions;
        [FormerlySerializedAs("CompileDefineRemovals")]
        public List<string> compileDefineRemovals;
        public List<KeyValuePair<LogType, StackTraceLogType>> LogLevels;

        public static ScreenOrientation DefaultScreenOrientation(ViitorCloudBuildConfigTarget target) {
            switch (target) {
                case ViitorCloudBuildConfigTarget.Android:
                    return ScreenOrientation.AutoRotation;

                case ViitorCloudBuildConfigTarget.iOS:
                    return ScreenOrientation.Portrait;

                case ViitorCloudBuildConfigTarget.Windows:
                case ViitorCloudBuildConfigTarget.OSX:
                default:
                    LogError(string.Format("[ViitorCloudBuildConfigObject] Unknown build target: {0}", target));
                    return ScreenOrientation.AutoRotation;
            }
        }

        public string VerboseName(string debugName, string releaseName, string betaName) {
            string typeName = releaseName;
            if (isBetaBuild) {
                typeName = betaName;
            } else if (isDevelopmentBuild) {
                typeName = debugName;
            }

            return target switch {
                ViitorCloudBuildConfigTarget.Windows => $"{(string.IsNullOrEmpty(productName) ? Application.productName : productName)}-{Application.version}",
                ViitorCloudBuildConfigTarget.iOS => $"{(string.IsNullOrEmpty(productName) ? Application.productName : productName)}-{Application.version}-{target}-{typeName}-{PlayerSettings.iOS.buildNumber}",
                _ => $"{(string.IsNullOrEmpty(productName) ? Application.productName : productName)}-{Application.version}-{target}-{typeName}"
            };
        }

        public override string ToString() {
            return $"{name} ({target})";
        }

        public string ToVerboseString() {
            return string.Format("Name={0}, Target={1}, Dev={2}, ProductName={3}, BundleId={4}, BuildPath={5}, IconPath={6}, Orientation={7}, Scenes={8}, CompileAdd={9}, CompileRem={10}",
                name,
                target,
                isDevelopmentBuild,
                productName,
                bundleId,
                buildPath,
                iconPath,
                (portrait || portraitUpsideDown) && (landscapeLeft || landscapeRight) ? "AutoRotation" : portrait ? "Portrait" : "Landscape",
                scenes == null ? "null" : string.Join(",", ExtractSceneName(scenes).ToArray()),
                compileDefineAdditions == null ? "null" : string.Join(",", compileDefineAdditions.ToArray()),
                compileDefineRemovals == null ? "null" : string.Join(",", compileDefineRemovals.ToArray())
            );

            List<string> ExtractSceneName(List<SceneAssets> sceneAssetsList) {
                // Use LINQ to get the names of all scenes in the build settings
                return Enumerable.Range(0, sceneAssetsList.Count)
                    .Select(index => sceneAssetsList[index].scene.name)
                    .ToList();
            }
        }

        private void EnsureAbsent(string flag) {
            Log(string.Format("[ViitorCloudBuildConfig] EnsureAbsent({0})", flag));

            if (compileDefineAdditions.Contains(flag)) {
                Log(string.Format("[ViitorCloudBuildConfig] CompileDefineAdditions.Remove({0})", flag));
                compileDefineAdditions.Remove(flag);
            }

            if (!compileDefineRemovals.Contains(flag)) {
                Log(string.Format("[ViitorCloudBuildConfig] CompileDefineRemovals.Add({0})", flag));
                compileDefineRemovals.Add(flag);
            }
        }

        private void EnsurePresent(string flag) {
            Log(string.Format("[ViitorCloudBuildConfig] EnsurePreset({0})", flag));

            if (!compileDefineAdditions.Contains(flag)) {
                Log(string.Format("[ViitorCloudBuildConfig] CompileDefineAdditions.Add({0})", flag));
                compileDefineAdditions.Add(flag);
            }

            if (compileDefineRemovals.Contains(flag)) {
                Log(string.Format("[ViitorCloudBuildConfig] CompileDefineRemovals.Remove({0})", flag));
                compileDefineRemovals.Remove(flag);
            }
        }

        public void Clean() {
#if UNITY_EDITOR
            // Do not use Debug.isDebugBuild, it is always true in editor
            bool devBuild = EditorUserBuildSettings.development;
            Log(string.Format("[ViitorCloudBuildConfig] EditorUserBuildSettings.development = {0}", EditorUserBuildSettings.development));
#else
			bool devBuild = Debug.isDebugBuild;
			Log(string.Format("[ViitorCloudBuildConfig] Debug.isDebugBuild = {0}", Debug.isDebugBuild));
#endif

#if QA
			devBuild = true;
			Log("[ViitorCloudBuildConfig] devBuild = true (FORCED because of QA define)");
#elif BETA
			devBuild = true;
			Log("[ViitorCloudBuildConfig] devBuild = true (FORCED because of BETA define)");
#else
            devBuild |= isDevelopmentBuild || isBetaBuild;
            Log(string.Format("[ViitorCloudBuildConfig] devBuild = {0} |= IsDevelopmentBuild={1} || IsBetaBuild={2}", devBuild, isDevelopmentBuild, isBetaBuild));
#endif

            compileDefineAdditions ??= new List<string>();

            compileDefineRemovals ??= new List<string>();

            if (devBuild) {
#if BETA
				EnsurePresent("BETA");
#else
                EnsurePresent("QA");
#endif

                EnsureAbsent("RELEASE");
            } else {
                EnsurePresent("RELEASE");

                EnsureAbsent("QA");
                EnsureAbsent("BETA");
                EnsureAbsent("DEBUG");
                EnsureAbsent("AUTOMATIC_CAPTURES");
            }
        }

        public ViitorCloudBuildConfigObject(string name, ViitorCloudBuildConfigTarget target) {
            this.name = name;
            this.target = target;
        }
    }
}

#endif