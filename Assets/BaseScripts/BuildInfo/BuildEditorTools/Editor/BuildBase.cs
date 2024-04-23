#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;

using UnityEngine;
using UnityEngine.Rendering;

using ViitorCloud.Base.BaseScripts.BuildObjects.Editor;
using ViitorCloud.Base.BaseScripts.Internal.Scripts.Models.Data.GameInfo;

using static UnityEditor.PlayerSettings;

using static Modules.Utility.Utility;

namespace ViitorCloud.Base.BaseScripts.BuildInfo.BuildEditorTools.Editor {

    public static class BuildBase {

        public static ViitorCloudBuildConfigObject FindConfig(ViitorCloudBuildConfigTarget target, string name, string prefix = null) {
            Log($"[BuildBase] FindConfig(manifest=NONE, target={target}, name={name}, prefix={prefix})");

            Log("[BuildBase] Finding Config");
            ViitorCloudBuildConfigObject config = ViitorCloudBuildData.GetConfig(target, name, prefix);
            if (config != null)
                return config;
            Log("[BuildBase] Finding Backup Config by target name");
            config = ViitorCloudBuildData.GetConfig(target, name, target.ToString() + "-");

            return config;
        }

        private static void StoreBuildDefinesForTarget(BuildTargetGroup targetGroup) {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            string path = Path.Combine(Application.dataPath, CompileDefines.Filename);

            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) {
                if (dir != null)
                    Directory.CreateDirectory(dir);
            }

            Log(string.Format("[BuildBase] StoreBuildDefines: {0} = {1}", CompileDefines.Filename, defines));

            File.WriteAllText(path, defines);
        }

        // Tried this through PBX, but didn't seem to work?
        // ReSharper disable once CommentTypo
        // https://gist.github.com/tenpn/f8da1b7df7352a1d50ff#file-symbolicationenabler-cs
        [PostProcessBuild(1400)] // near the end
        public static void OnPostProcessBuild(BuildTarget target, string path) {
            if (target != BuildTarget.iOS) {
                return;
            }

            // Adjust dSYM generation
            string xcodeProjectPath = Path.Combine(path, "Unity-iPhone.xcodeproj");
            string pbxPath = Path.Combine(xcodeProjectPath, "project.pbxproj");

            StringBuilder sb = new StringBuilder();

            string[] xcodeProjectLines = File.ReadAllLines(pbxPath);
            foreach (string line in xcodeProjectLines) {
                // Remove from OTHER_LDFLAGS
                if (line.Contains("-Wl,-S,-x")) {
                    continue;
                }

                // iOS Default when not present is dwarf-with-dsym
                if (line.Contains("DEBUG_INFORMATION_FORMAT")) {
                    // Replace line to stripping on postprocess deployment flags
                    sb.AppendLine("\t\t\t\tDEPLOYMENT_POSTPROCESSING = YES;");
                    sb.AppendLine("\t\t\t\tSEPARATE_STRIP = YES;");
                    sb.AppendLine("\t\t\t\tSTRIP_INSTALLED_PRODUCT = YES;");
                    continue;
                }

                // Defaults to Yes
                if (line.Contains("COPY_PHASE_STRIP")) {
                    continue;
                }

                // Defaults to Yes
                if (line.Contains("GCC_GENERATE_DEBUGGING_SYMBOLS")) {
                    continue;
                }

                sb.AppendLine(line);
            }

            File.WriteAllText(pbxPath, sb.ToString());

#if UNITY_2019_3_OR_NEWER
            string mainAppPath = Path.Combine(path, "MainApp", "main.mm");
            string mainContent = File.ReadAllText(mainAppPath);
            string newMainContent = mainContent.Replace("#include <UnityFramework/UnityFramework.h>", @"#include ""../UnityFramework/UnityFramework.h""");
            File.WriteAllText(mainAppPath, newMainContent);

            string unitTestPath = Path.Combine(path, "Unity-iPhone Tests", "Unity_iPhone_Tests.m");
            string unitTestContent = File.ReadAllText(unitTestPath);
            string newUnitTestContent = unitTestContent.Replace("#include <UnityFramework/UnityFramework.h>", @"#include ""../UnityFramework/UnityFramework.h""");
            File.WriteAllText(unitTestPath, newUnitTestContent);
#endif
        }

        private static string[] GetValidScenes(List<SceneAssets> scenes) {
            List<string> validScenes = new List<string>();

            if (scenes is not { Count: > 0 })
                return validScenes.Count == 0 ? null : validScenes.ToArray();
            validScenes.AddRange(from scene in scenes where scene.active select AssetDatabase.GetAssetPath(scene.scene));

            return validScenes.Count == 0 ? null : validScenes.ToArray();
        }

        public static void BuildTo(ViitorCloudBuildConfigObject config) {
            if (config == null) {
                LogError("ERROR: Cannot BuildTo null config");
                return;
            }

            string path = GetPath(config);

            // delete stuff at the build path
            Log(string.Format("[BuildBase] Cleaning: {0}", path));
            if (Directory.Exists(path)) {
                Directory.Delete(path, true);
            }

            // create the build dir
            Log(string.Format("[BuildBase] Creating build dir: {0}", path));
            Directory.CreateDirectory(path);

            string filePath = Path.Combine(path, GetFilename(config));

            string[] scenes = GetValidScenes(config.scenes);

            //string[] editorScenes = new string[EditorBuildSettings.scenes.Length];
            //for (int i = 0; i < editorScenes.Length; i++)
            //{
            //    editorScenes[i] = EditorBuildSettings.scenes[i].path;
            //}
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions {
                scenes = scenes,
                locationPathName = filePath,
                target = GetBuildTarget(config),
                options = BuildOptions.None
            };

            if (config.isDevelopmentBuild) {
                buildPlayerOptions.options |= BuildOptions.CompressWithLz4;
                buildPlayerOptions.options |= BuildOptions.Development;
            } else {
                buildPlayerOptions.options |= BuildOptions.CompressWithLz4HC;
                buildPlayerOptions.options |= BuildOptions.CleanBuildCache;
            }

            Log(string.Format("[BuildBase] BuildPlayerOptions.options : {0}", buildPlayerOptions.options));
            Log(string.Format("[BuildBase] BuildPlayerOptions.target : {0}", buildPlayerOptions.target));
            Log(string.Format("[BuildBase] BuildPlayerOptions.locationPathName : {0}", filePath));
            Log(string.Format("[BuildBase] BuildPlayerOptions.scenes : [{0}]", buildPlayerOptions.scenes == null ? "null" : string.Join(",", buildPlayerOptions.scenes)));

            SafeAssetDatabase.Refresh();

            switch (config.target) {
                case ViitorCloudBuildConfigTarget.Android:
                    BuildBaseAndroid.PreBuildPlayer();
                    break;

                case ViitorCloudBuildConfigTarget.Windows:
                    BuildBaseWindows.PreBuildPlayer();
                    break;

                case ViitorCloudBuildConfigTarget.OSX:
                    BuildBaseWindows.PreBuildPlayer();
                    break;
                case ViitorCloudBuildConfigTarget.iOS:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            string buildPath = Path.Combine(Application.dataPath.Replace(@"\Assets", "").Replace(@"/Assets", ""), "BuildPath.txt");
            Log("buildPath : " + buildPath);
            if (!File.Exists(buildPath)) {
                FileStream fileStream = new FileStream(buildPath, FileMode.CreateNew, FileAccess.ReadWrite);
                fileStream.Close();
            }

            File.WriteAllText(buildPath, Path.Combine(Application.dataPath.Replace(@"\Assets", "").Replace(@"/Assets", ""), filePath));

            Log("[BuildBase] Starting build...");
            BuildReport buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = buildReport.summary;

            switch (summary.result) {
                case BuildResult.Succeeded: {
                    Log($"[BuildBase] Build succeeded: {summary.totalSize} bytes in {summary.totalTime} time.");
                    Log($"path {summary.outputPath}");
                    Log($"config.LaunchExecutable {config.launchExecutable}");
                    if (config.launchExecutable) {
                        if (config.target == ViitorCloudBuildConfigTarget.Windows) {
                            Application.OpenURL(summary.outputPath);
                        } else {
                            System.Diagnostics.Process process = new System.Diagnostics.Process();
                            process.StartInfo.FileName = Path.GetDirectoryName(summary.outputPath) ?? string.Empty;
                            process.Start();
                        }
                    }
                    break;
                }

                case BuildResult.Failed:
                    LogError(string.Format("[BuildBase] Build failed with {summary.totalErrors} errors in {summary.totalTime} time"));
                    break;
                case BuildResult.Unknown:
                    break;
                case BuildResult.Cancelled:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        private static void SetAppId(string appId, BuildTargetGroup targetGroup = BuildTargetGroup.iOS) {
            if (string.IsNullOrEmpty(appId)) {
                return;
            }

            Log(string.Format("[BuildBase] SetAppId({0})", appId));
            PlayerSettings.SetApplicationIdentifier(targetGroup, appId);
        }

        private static void SetProductName(string productName) {
            if (string.IsNullOrEmpty(productName)) {
                return;
            }

            Log(string.Format("[BuildBase] SetProductName({0})", productName));
            PlayerSettings.productName = productName;
        }

        private static void SetCompanyName(string companyName) {
            if (string.IsNullOrEmpty(companyName)) {
                return;
            }

            Log(string.Format("[BuildBase] SetCompanyName({0})", companyName));
            PlayerSettings.companyName = companyName;
        }

        public static void SetProductVersion(string productVersion) {
            if (string.IsNullOrEmpty(productVersion)) {
                return;
            }

            Log(string.Format("[BuildBase] SetProductVersion({0})", productVersion));
            PlayerSettings.bundleVersion = productVersion;
        }

        private static void SetSplashScreen(bool splashBgBlur, string splashBg = "", string iconPath = "") {
            Log(string.Format("[BuildBase] SetSplashScreen({0})", iconPath));
            PlayerSettings.SplashScreen.showUnityLogo = false;
            if (iconPath != null) {
                Sprite icon = AssetDatabase.LoadAssetAtPath(iconPath, typeof(Sprite)) as Sprite;
                SplashScreenLogo splashScreenLogo = new SplashScreenLogo {
                    logo = icon,
                    duration = 1
                };

                PlayerSettings.SplashScreen.logos = new[] {
                    splashScreenLogo
                };
                PlayerSettings.SplashScreen.show = true;
            } else {
                PlayerSettings.SplashScreen.logos = null;
            }

            if (splashBg != null) {
                Sprite icon = AssetDatabase.LoadAssetAtPath(splashBg, typeof(Sprite)) as Sprite;
                PlayerSettings.SplashScreen.background = icon;
                PlayerSettings.SplashScreen.blurBackgroundImage = splashBgBlur;
            }
        }

        private static void SetIcon(string iconPath, NamedBuildTarget targetGroup) {
            if (string.IsNullOrEmpty(iconPath)) {
                return;
            }

            Log(string.Format("[BuildBase] SetIcon({0})", iconPath));

            Texture2D icon = AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture2D)) as Texture2D;
#if UNITY_STANDALONE
            Texture2D[] icons = PlayerSettings.GetIcons(targetGroup, IconKind.Application);
#else
            Texture2D[] icons = PlayerSettings.GetIcons(targetGroup, IconKind.Any);
#endif
            int numIcons = icons.Length;
            for (int i = 0; i < numIcons; i++) {
                icons[i] = icon;
            }

#if UNITY_STANDALONE
            SetIcons(targetGroup, icons, IconKind.Application);
#else
            PlayerSettings.SetIcons(targetGroup, icons, IconKind.Any);
#endif
        }

        private static BuildTargetGroup GetBuildTargetGroup(ViitorCloudBuildConfigObject config) {
            BuildTargetGroup targetGroup = BuildTargetGroup.iOS;

            switch (config.target) {
                case ViitorCloudBuildConfigTarget.iOS:
                    targetGroup = BuildTargetGroup.iOS;
                    break;

                case ViitorCloudBuildConfigTarget.Android:
                    targetGroup = BuildTargetGroup.Android;
                    break;

                case ViitorCloudBuildConfigTarget.Windows:
                    targetGroup = BuildTargetGroup.Standalone;
                    break;

                case ViitorCloudBuildConfigTarget.OSX:
                    targetGroup = BuildTargetGroup.Standalone;
                    break;

                default:
                    LogError(string.Format("[BuildBase] Unknown build target: {0} for {1}", config.target, config.name));
                    break;
            }
            Log(string.Format("[BuildBase] Setting build target: {0} for {1}", config.target, config.name));
            return targetGroup;
        }

        private static NamedBuildTarget GetNamedBuildTarget(ViitorCloudBuildConfigObject config) {
            NamedBuildTarget targetGroup = NamedBuildTarget.iOS;

            switch (config.target) {
                case ViitorCloudBuildConfigTarget.iOS:
                    targetGroup = NamedBuildTarget.iOS;
                    break;

                case ViitorCloudBuildConfigTarget.Android:
                    targetGroup = NamedBuildTarget.Android;
                    break;

                case ViitorCloudBuildConfigTarget.Windows:
                    targetGroup = NamedBuildTarget.Standalone;
                    break;

                case ViitorCloudBuildConfigTarget.OSX:
                    targetGroup = NamedBuildTarget.Standalone;
                    break;

                default:
                    LogError(string.Format($"[BuildBase] Unknown build target: {0} for {1}", config.target, config.name));
                    break;
            }
            Log(string.Format("[BuildBase] Setting build target: {0} for {1}", config.target, config.name));
            return targetGroup;
        }

        private static BuildTarget GetBuildTarget(ViitorCloudBuildConfigObject config) {
            BuildTarget target = BuildTarget.iOS;

            switch (config.target) {
                case ViitorCloudBuildConfigTarget.iOS:
                    target = BuildTarget.iOS;
                    break;

                case ViitorCloudBuildConfigTarget.Android:
                    target = BuildTarget.Android;
                    break;

                case ViitorCloudBuildConfigTarget.Windows:
                    target = BuildTarget.StandaloneWindows64;
                    break;

                case ViitorCloudBuildConfigTarget.OSX:
                    target = BuildTarget.StandaloneOSX;
                    break;

                default:
                    LogError( string.Format("[BuildBase] Unknown build target: {0} for {1}", config.target, config.name));
                    break;
            }

            return target;
        }

        private static string GetPath(ViitorCloudBuildConfigObject config) {
            if (!string.IsNullOrEmpty(config.buildPath)) {
                return config.buildPath;
            }

            string baseDir = ViitorCloudBuildData.BuildFolderBase;
            return Path.Combine(baseDir, config.target.ToString(), Path.GetFileNameWithoutExtension(GetFilename(config)));
        }

        private static string GetFilename(ViitorCloudBuildConfigObject config) {
            string verboseName = config.VerboseName(ViitorCloudBuildData.KDebugBuildName, ViitorCloudBuildData.KReleaseBuildName, ViitorCloudBuildData.KBetaBuildName);

            switch (config.target) {
                case ViitorCloudBuildConfigTarget.iOS:
                    return verboseName;

                case ViitorCloudBuildConfigTarget.Android:
                    return verboseName + ".apk";

                case ViitorCloudBuildConfigTarget.Windows:
                    return verboseName + ".exe";

                case ViitorCloudBuildConfigTarget.OSX:
                    return verboseName;
            }

            LogError(string.Format("[BuildBase] Unknown target type: {config.target}"));
            return null;
        }

        public static void PreBuild(ViitorCloudBuildConfigObject config, bool updateDefines = true) {
            Log(string.Format("[BuildBase] PreBuild({0})", config));

            // Games can hook into build process and run before ViitorCloud's PreBuild scripts
            ViitorCloudPreBuildUtil.Run(typeof(ViitorCloudBeforePreBuild), config);

            BuildTargetGroup group = GetBuildTargetGroup(config);
            NamedBuildTarget namedGroup = GetNamedBuildTarget(config);

            if (updateDefines) {
                UpdateBuildDefines(config.compileDefineAdditions, config.compileDefineRemovals, config.isDevelopmentBuild, config.isBetaBuild, group);
            }

            StoreBuildDefinesForTarget(group);

            UpdateLoggingLevels(config);
            UpdateAndroidManifest();

            if (group == BuildTargetGroup.Android) {
                if (ViitorCloudBuildData.AutoUpdateAndroidVersion) {
                    //BuildBaseAndroid.UpdateBuildNumber();
                }
            } else if (group == BuildTargetGroup.iOS) {
                if (ViitorCloudBuildData.AutoUpdateIOSVersion) {
                    BuildBaseIOS.UpdateBuildNumber();
                }
            }

            SetAppId(config.bundleId, group);
            SetProductName(config.productName);
            SetCompanyName(config.companyName);
            SetIcon(config.iconPath, namedGroup);
            SetSplashScreen(config.blurSplashBg, config.splashScreenLogoBg, config.splashScreenLogo);
            SetScreenParams(config);

            // Games can hook into build process and run after ViitorCloud's PreBuild scripts (but before the actual build still)
            ViitorCloudPreBuildUtil.Run(typeof(ViitorCloudAfterPreBuild), config);

            SafeAssetDatabase.Refresh();
        }

        private static void SetScreenParams(ViitorCloudBuildConfigObject config) {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;

            if (config.portrait) {
                PlayerSettings.allowedAutorotateToPortrait = config.portrait;
            } else if (config.landscapeLeft) {
                PlayerSettings.allowedAutorotateToLandscapeLeft = config.landscapeLeft;
            } else if (config.landscapeRight) {
                PlayerSettings.allowedAutorotateToLandscapeRight = config.landscapeRight;
            } else if (config.portraitUpsideDown) {
                PlayerSettings.allowedAutorotateToPortraitUpsideDown = config.portraitUpsideDown;
            }

            PlayerSettings.allowedAutorotateToPortrait = config.portrait;
            PlayerSettings.allowedAutorotateToLandscapeLeft = config.landscapeLeft;
            PlayerSettings.allowedAutorotateToLandscapeRight = config.landscapeRight;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = config.portraitUpsideDown;
        }

        private static void EnsureExists(string define, List<string> toAdd, List<string> toRemove) {
            if (!toAdd.Contains(define)) {
                toAdd.Add(define);
            }

            if (toRemove.Contains(define)) {
                toRemove.Remove(define);
            }
        }

        private static void EnsureAbsent(string define, List<string> toAdd, List<string> toRemove) {
            if (toAdd.Contains(define)) {
                toAdd.Remove(define);
            }

            if (!toRemove.Contains(define)) {
                toRemove.Add(define);
            }
        }

        private static void UpdateBuildDefines(List<string> toAdd, List<string> toRemove, bool devBuild, bool betaBuild, BuildTargetGroup targetGroup = BuildTargetGroup.iOS) {
            string currentDefines = GetScriptingDefineSymbolsForGroup(targetGroup);
            Log(string.Format("[BuildBase] Current scripting define symbols for {0}: {1}", targetGroup, currentDefines));

            toAdd ??= new List<string>();

            toRemove ??= new List<string>();

            if (devBuild) {
                EnsureAbsent("RELEASE", toAdd, toRemove);
            } else if (betaBuild) {
                EnsureAbsent("RELEASE", toAdd, toRemove);
                EnsureAbsent("QA", toAdd, toRemove);

                EnsureExists("BETA", toAdd, toRemove);
            } else {
                EnsureAbsent("QA", toAdd, toRemove);
                EnsureAbsent("BETA", toAdd, toRemove);
                EnsureAbsent("AUTOMATIC_CAPTURES", toAdd, toRemove);

                EnsureExists("RELEASE", toAdd, toRemove);
            }

            List<string> modifiedDefines = new List<string>();
            foreach (string currentDefine in currentDefines.Split(new[] {
                         ';'
                     }, StringSplitOptions.RemoveEmptyEntries)) {
                if (toRemove.Contains(currentDefine))
                    continue;
                Log(string.Format("[BuildBase] Adding symbol: {0}", currentDefine));
                modifiedDefines.Add(currentDefine);
            }

            foreach (string ensureDefine in toAdd.Where(ensureDefine => !modifiedDefines.Contains(ensureDefine))) {
                Log(string.Format("[BuildBase] Ensuring symbol: {0}", ensureDefine));
                modifiedDefines.Add(ensureDefine);
            }

            if (modifiedDefines.Contains("QA") && modifiedDefines.Contains("RELEASE")) {
                LogError(string.Format( "[BuildBase] ERROR: Defines contain both QA and RELEASE!!! [{0}]", string.Join(",", modifiedDefines.ToArray())));
                modifiedDefines.Remove("RELEASE");
                modifiedDefines.Remove("QA");
            }

            string finalDefines = string.Join(";", modifiedDefines.ToArray());

            Log(string.Format("[BuildBase] Setting scripting define symbols: {0}", finalDefines));
            SetScriptingDefineSymbolsForGroup(targetGroup, finalDefines);
            Log(string.Format("[BuildBase] Final scripting define symbols: {0}", GetScriptingDefineSymbolsForGroup(targetGroup)));
        }

        private static void UpdateLoggingLevels(ViitorCloudBuildConfigObject config) {
            // Cleaner logs for device builds, while may be helpful to keep full traces in editor
            SetStackTraceLogType(LogType.Log, StackTraceLogType.ScriptOnly);
            SetStackTraceLogType(LogType.Warning, StackTraceLogType.ScriptOnly);
            SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
            SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
            SetStackTraceLogType(LogType.Assert, StackTraceLogType.Full);

            // If we have overrides, use them
            if (config.LogLevels == null)
                return;
            foreach (KeyValuePair<LogType, StackTraceLogType> kvp in config.LogLevels) {
                SetStackTraceLogType(kvp.Key, kvp.Value);
            }
        }

        private static void UpdateAndroidManifest() {
#if (UNITY_ANDROID && HIDEAPP)
			// For now we will only consider the primary manifest, not any secondary ones
			string path = Path.Combine(Application.dataPath, "Plugins/Android/AndroidManifest.xml");
			if (!File.Exists(path)) {
				LogError(string.Format("[BuildBase] Cannot update AndroidManifest file, no such file in: {0}", path);
				return;
			}

			string manifest = null;
			try {
				manifest = File.ReadAllText(path);
			} catch (Exception e) {
				LogError(string.Format("[BuildBase] Cannot load AndroidManifest file: {0}", e.ToString());
			}

			if (string.IsNullOrEmpty(manifest)) {
				return;
			}

			Regex regex = new Regex("<intent-filter>[\\w\\s]*<action android:name=\"android.intent.action.MAIN\"[\\w\\s]*/>[\\w\\s]*<category android:name=\"android.intent.category.LAUNCHER\"[\\w\\s]*/>[\\w\\s]*</intent-filter>");
			Match match = regex.Match(manifest);

			if (!match.Success) {
				Log("[BuildBase] No MAIN/LAUNCHER intent-filter to remove for K3 hidden app");
				return;
			}

			LogWarningFormat("[BuildBase] Removing MAIN/LAUNCHER intent-filter");
			manifest = regex.Replace(manifest, "<!--REMOVED MAIN LAUNCHER intent-filter DO NOT COMMIT THIS-->");

			try {
				File.WriteAllText(path, manifest);
			} catch (Exception e) {
				LogError(string.Format("[BuildBase] Could not write AndroidManifest: {0}", e.ToString());
			}
#endif
        }

        public static void AddAlwaysIncludedShader(string shaderName) {
            Shader shader = Shader.Find(shaderName);
            if (shader == null) {
                LogError(string.Format("Could not find {0} shared to include into m_AlwaysIncludedShaders", shaderName));
                return;
            }

            GraphicsSettings graphicsSettingsObj = AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
            SerializedObject serializedObject = new SerializedObject(graphicsSettingsObj);
            SerializedProperty arrayProp = serializedObject.FindProperty("m_AlwaysIncludedShaders");
            bool hasShader = false;

            for (int i = 0; i < arrayProp.arraySize; ++i) {
                SerializedProperty arrayElem = arrayProp.GetArrayElementAtIndex(i);
                if (shader != arrayElem.objectReferenceValue)
                    continue;
                hasShader = true;
                break;
            }

            if (hasShader)
                return;
            {
                int arrayIndex = arrayProp.arraySize;
                arrayProp.InsertArrayElementAtIndex(arrayIndex);
                SerializedProperty arrayElem = arrayProp.GetArrayElementAtIndex(arrayIndex);
                arrayElem.objectReferenceValue = shader;

                serializedObject.ApplyModifiedProperties();

                AssetDatabase.SaveAssets();
            }
        }

        public static string GetBuildNumber() {
            return iOS.buildNumber;
        }
    }
}

#endif