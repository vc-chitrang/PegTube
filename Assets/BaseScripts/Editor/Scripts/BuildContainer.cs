#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEditor;

using UnityEngine;

using static Modules.Utility.Utility;

using ViitorCloud.Base.BaseScripts.BuildInfo.BuildEditorTools.Editor;
using ViitorCloud.Base.BaseScripts.BuildObjects.Editor;
using ViitorCloud.Base.BaseScripts.Internal.Editor.GameConfig;
using ViitorCloud.Base.BaseScripts.Internal.Scripts.Models.Data.GameInfo;
using ViitorCloud.Container.Internal.Data;
namespace ViitorCloud.Base.BaseScripts.Editor.Scripts {

    public static class BuildContainer {
        //private static ViitorCloudGameInfo kCombinedGameInfo = null;

        public static void StartBuild(ViitorCloudBuildConfigTarget target, string name, bool isStandalone, bool isDevelopmentBuild) {
            if (string.IsNullOrEmpty(name)) {
                name = "Base-Project";
            }

            ViitorCloudBuildConfigObject config = PreBuild(target, name, isStandalone, isDevelopmentBuild);

            StartBuild(config);
        }

        private static void StartBuild(ViitorCloudBuildConfigObject config) {
            BuildBase.BuildTo(config);
        }

        public static ViitorCloudBuildConfigObject PreBuild(ViitorCloudBuildConfigTarget target, string name, bool isStandalone, bool isDevelopmentBuild) {
            if (string.IsNullOrEmpty(name)) {
                name = "Base-Project";
            }

            Log(string.Format("[BuildContainer] PreBuild(target={0}, name={1}, standalone={2}, development={3})", target, name, isStandalone, isDevelopmentBuild));

            GameConfigManager manager = new GameConfigManager();
            ViitorCloudBuildConfigObject config = new ViitorCloudBuildConfigObject(name, target);

            LoadConfigData(manager, config, isDevelopmentBuild);

            string primaryGameID = null;
            string[] gameIDs = manager.AvailableGameIds.ToArray();

            List<SceneAssets> initialScenesStrings = config.scenes.Where(scenePath => scenePath.active).ToList();

            config.scenes = initialScenesStrings;

            ViitorCloudBuildData.AutoUpdateAndroidVersion = true;

            // Find valid games we're building
            Dictionary<string, ViitorCloudGameInfo> usedGameInfo = new Dictionary<string, ViitorCloudGameInfo>();
            foreach (string gameID in gameIDs) {
                Log(string.Format("[BuildContainer] PreBuild({0})", gameID));

                ViitorCloudGameInfoSo gameInfoSo = manager.GetGameInfoSo(gameID);
                if (gameInfoSo == null) {
                    LogError(string.Format("[BuildContainer] Could not find non-null GameInfoSO for: {0}", gameID));
                    continue;
                }

                Log(string.Format("[BuildContainer] PreBuild({0}) GameConfig: {1}", gameID, gameInfoSo.name));

                ViitorCloudGameInfo gameInfo = gameInfoSo.value;
                if (gameInfo == null) {
                    LogError(string.Format("[BuildContainer] Could not find non-null GameInfo for: {0} from {1}", gameID, gameInfoSo.name));
                    continue;
                }

                usedGameInfo[gameInfoSo.name] = gameInfo;
            }

            // Ensure metadata all loaded
            manager.Reload(manager.PrimaryGameId);

            // Run per-game prebuild code
            foreach (string gameInfoSoName in usedGameInfo.Keys) {
                ViitorCloudGameInfo gameInfo = usedGameInfo[gameInfoSoName];

                Log(string.Format("[BuildContainer] PreBuild({0}) GameConfig: {1}, Initial Game Path: {2}", gameInfo.gameID, gameInfoSoName, gameInfo.gamePath));

                bool isPrimary = manager.PrimaryGameId == gameInfo.gameID;
                if (isPrimary) {
                    primaryGameID = gameInfo.gameID;
                }

                IncludedGameConfigPreBuild(config, gameInfo);

                Log(string.Format("{0}.Scenes = ", gameInfo.gameID));
                for (int i = 0; i < gameInfo.scenes.Count; i++) {
                    if (gameInfo.scenes[i].active) {
                        Log(string.Format("{0} {1}", i, gameInfo.scenes[i].scene.name));
                    }
                }
            }

            // Run primary game prebuild code after others, so it can overwrite any values it needs to
            if (primaryGameID == null) {
                LogError(string.Format("[BuildContainer] Could not find any primary game id game config! {0}", manager.PrimaryGameId));
            } else {
                ViitorCloudGameInfoSo primaryGameInfoSo = manager.GetGameInfoSo(primaryGameID);
                if (primaryGameInfoSo == null) {
                    LogError(string.Format("[BuildContainer] Could not find scriptable object for primary game id: {0}", primaryGameID));
                } else {
                    PrimaryGameConfigPreBuild(config, primaryGameInfoSo.value);
                }
            }

            // Set project level values
            SetPlayerSettings(manager.GetGameInfo(), primaryGameID);
            SetURLSchemes(manager.GetGameInfo(), target);

            // Use the gameID as the config name because otherwise it will just be a generic one
            config.name = primaryGameID;

            // Run shared prebuild
            BuildBase.PreBuild(config);

            switch (target) {
                case ViitorCloudBuildConfigTarget.Android:
                    PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_4_6);
                    break;
                case ViitorCloudBuildConfigTarget.iOS:
                    PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.iOS, ApiCompatibilityLevel.NET_4_6);
                    break;
                case ViitorCloudBuildConfigTarget.Windows:
                    break;
                case ViitorCloudBuildConfigTarget.OSX:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }

            return config;
        }

        private static void SetAddrassable(ViitorCloudGameInfo gameInfo) {
            if (gameInfo.packages.addressables) {
                PackagesManifestManager.AddAddressablsPackage();
            }
        }

        private static void SetUrpSettings(ViitorCloudGameInfo gameInfo) {
            if (!gameInfo.packages.urp)
                return;
            PackagesWindow.SetUpURP();
            PackagesWindow.SetupRPAsset(gameInfo);
        }

        private static void SetHdrpSettings(ViitorCloudGameInfo gameInfo) {
            if (!gameInfo.packages.hdrp)
                return;
            PackagesWindow.SetUpHDRP();
            PackagesWindow.SetupRPAsset(gameInfo);
        }

        private static void InstallARPackages(ViitorCloudGameInfo gameInfo) {
            if (gameInfo.packages.ar) {
                PackagesManifestManager.AddARPackages();
            }
        }

        private static void InstallVRPackages(ViitorCloudGameInfo gameInfo) {
            if (gameInfo.packages.vr) {
                PackagesManifestManager.AddVRPackages();
            }
        }

        private static void SetGraphicsColorProfile(ViitorCloudGameInfo gameInfo) {
            PlayerSettings.colorSpace = gameInfo.projectSettings.colorSpace;
        }

        private static void SetScriptingDefines() {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);

            List<string> symbolsToAdd = new List<string>() {
                "ViitorCloud"
            };

            string symbolString = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            Log(string.Format("[BuildContainer] Existing symbols for {0}/{1}: {2}", target, group, symbolString));

            HashSet<string> symbols = string.IsNullOrEmpty(symbolString) ? new HashSet<string>() : new HashSet<string>(symbolString.Split(new[] {
                ';'
            }, StringSplitOptions.RemoveEmptyEntries));

            foreach (string symbolToAdd in symbolsToAdd.Where(symbolToAdd => !string.IsNullOrEmpty(symbolToAdd) && !symbols.Contains(symbolToAdd))) {
                Log(string.Format("[BuildContainer] Appending symbol: {0}", symbolToAdd));
                symbols.Add(symbolToAdd);
            }

            symbolString = string.Join(";", symbols);

            Log(string.Format("[BuildContainer] Setting scripting define symbols: {0}", symbolString));
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, symbolString);
            Log(string.Format("[BuildContainer] Final scripting define symbols: {0}", PlayerSettings.GetScriptingDefineSymbolsForGroup(group)));
        }

        private static void SetDeviceSupport(ViitorCloudGameInfo gameInfo) {
#if UNITY_IOS
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            Log(string.Format("[BuildContainer] Setting iOS device support = {0}", PlayerSettings.iOS.targetDevice));

            if (string.IsNullOrEmpty(gameInfo.properties.MinIOSVersionSupported))
                return;
            PlayerSettings.iOS.targetOSVersionString = gameInfo.properties.MinIOSVersionSupported;
            Log(string.Format("[BuildContainer] Setting iOS min version support = {0}", PlayerSettings.iOS.targetOSVersionString));
#endif
        }

        private static void SetVersioning(ViitorCloudGameInfo gameInfo, string primaryGameId) {
            if (string.IsNullOrEmpty(gameInfo.properties.version)) {
                LogWarning(string.Format("[BuildContainer] Missing Properties.Version for '{0}' GameConfig", primaryGameId));
            } else {
                PlayerSettings.bundleVersion = gameInfo.properties.version;
                Log(string.Format("[BuildContainer] Set version number: {0}", PlayerSettings.bundleVersion));
            }
        }

        private static void SetURLSchemes(ViitorCloudGameInfo gameInfo, ViitorCloudBuildConfigTarget target) {
            if (gameInfo.properties.urlSchemes?.Count == 0) {
                Log("[BuildContainer] no URL schemes defined, at least one should be set in a game config file if you need to call into the game via browser/URL");
            }

            if (gameInfo.properties.urlSchemes.Count == 0) {
                return;
            }

            if (target == ViitorCloudBuildConfigTarget.iOS) {
                // URL Schemes must be set in the ProjectSettings.asset file, no access from PlayerSettings class
                const string projectSettingsAssetPath = "ProjectSettings/ProjectSettings.asset";
                SerializedObject projectSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath(projectSettingsAssetPath)[0]);
                SerializedProperty urlSchemes = projectSettings.FindProperty("iOSURLSchemes");
                urlSchemes.ClearArray();

                for (int i = 0, l = gameInfo.properties.urlSchemes.Count; i < l; i++) {
                    urlSchemes.InsertArrayElementAtIndex(i);
                    SerializedProperty urlScheme = urlSchemes.GetArrayElementAtIndex(i);
                    urlScheme.stringValue = $"{gameInfo.properties.hostForSchemes}://{gameInfo.properties.urlSchemes[i]}";
                }

                projectSettings.ApplyModifiedProperties();
            } else {
                //unitydl://mylink
                // For Android, the URL Schemes are set in the AndroidManifest file.  Create it from a template and add a section for each URL scheme.
                const string manifestTemplatePath = "Assets/Templates/AndroidManifest.xml";
                const string urlSchemeTemplatePath = "Assets/Templates/AndroidManifest_urlscheme.xml";
                const string urlSchemeHostReplace = "REPLACEME_HOST";
                const string urlSchemeReplace = "REPLACEME_URLSCHEME";
                const string manifestReplace = "<REPLACEME_URLSCHEMES />";
                const string manifestPath = "Assets/Plugins/Android/AndroidManifest.xml";
                TextAsset manifestTemplate = AssetDatabase.LoadAssetAtPath<TextAsset>(manifestTemplatePath);
                string manifestTemplateTxt = manifestTemplate.text;
                TextAsset urlSchemeTemplate = AssetDatabase.LoadAssetAtPath<TextAsset>(urlSchemeTemplatePath);
                string urlSchemeTemplateTxt = urlSchemeTemplate.text.Replace(urlSchemeHostReplace, gameInfo.properties.hostForSchemes);
                string urlSchemeText = "";
                for (int i = 0, l = gameInfo.properties.urlSchemes.Count; i < l; i++) {
                    if (i > 0) {
                        urlSchemeText += "\n";
                    }
                    urlSchemeText += urlSchemeTemplateTxt.Replace(urlSchemeReplace, gameInfo.properties.urlSchemes[i]);
                }
                // if no url schemes are defined, use the template text so the manifest is at least in the right format
                if (string.IsNullOrEmpty(urlSchemeText)) {
                    urlSchemeText = string.Copy(urlSchemeText);
                }
                string manifestText = manifestTemplateTxt.Replace(manifestReplace, urlSchemeText);
                using StreamWriter sw = File.CreateText(manifestPath);
                sw.Write(manifestText);
            }
        }

        private static void SetPlayerSettings(ViitorCloudGameInfo gameInfo, string primaryGameId) {
            SetScriptingDefines();
            SetDeviceSupport(gameInfo);
            SetVersioning(gameInfo, primaryGameId);
            SetGraphicsColorProfile(gameInfo);
            if (!gameInfo.packages.doNotForceChangePipeline) {
                SetUrpSettings(gameInfo);
                SetHdrpSettings(gameInfo);
            }
            SetAddrassable(gameInfo);
            InstallARPackages(gameInfo);
            InstallVRPackages(gameInfo);
        }

        private static void IncludedGameConfigPreBuild(ViitorCloudBuildConfigObject config, ViitorCloudGameInfo gameInfo) {
            Log(string.Format("[BuildContainer] Updating assets for: {0}", gameInfo.gameID));

            AssetTool.IncludeScenesInBuild(gameInfo.scenes);
            Log(string.Format("[BuildContainer] Populating combined game config with settings for: {0}", gameInfo.gameID));

            config.scenes.AddRange(gameInfo.scenes);
        }

        private static void PrimaryGameConfigPreBuild(ViitorCloudBuildConfigObject config, ViitorCloudGameInfo gameInfo) {
            Log(string.Format("[BuildContainer] Updating private state with primary game config settings: {0}", gameInfo.gameID));

            Log(string.Format("[BuildContainer] Populating combined game config with settings for primary game: {0}", gameInfo.gameID));

            PlayerSettings.Android.bundleVersionCode = gameInfo.properties.versionCode;
            config.portrait = gameInfo.properties.portrait;
            config.portraitUpsideDown = gameInfo.properties.portraitUpsideDown;
            config.landscapeLeft = gameInfo.properties.landscapeLeft;
            config.landscapeRight = gameInfo.properties.landscapeRight;
        }

        private static void LoadConfigData(GameConfigManager manager, ViitorCloudBuildConfigObject config, bool isDevelopmentBuild) {
            Log(string.Format("[BuildContainer] Loading config data for: {0}", manager.PrimaryGameId));

            ViitorCloudGameInfo primaryGameConfig = manager.LoadGameConfigInfo(manager.PrimaryGameId, create: false);
            Log(string.Format("[BuildContainer] PrimaryGameID: {0}", primaryGameConfig.gameID));

            if (!string.IsNullOrEmpty(primaryGameConfig.properties.title)) {
                config.productName = primaryGameConfig.properties.title;
                config.companyName = primaryGameConfig.properties.companyName;
                Log(string.Format("[BuildContainer] ProductName: {0}", config.productName));
            } else {
                LogWarning($"[BuildContainer] Missing Properties.Title for '{primaryGameConfig.gameID}' GameConfig");
            }

            if (!string.IsNullOrEmpty(primaryGameConfig.properties.bundleId)) {
                if (isDevelopmentBuild) {
                    config.bundleId = primaryGameConfig.properties.bundleId + ".development";
                } else {
                    config.bundleId = primaryGameConfig.properties.bundleId;
                }
                Log(string.Format("[BuildContainer] BundleId: {0}", config.bundleId));
            } else {
                LogWarning(string.Format("[BuildContainer] Missing Properties.BundleID for '{0}' GameConfig", primaryGameConfig.gameID));
            }

            if (primaryGameConfig.properties.icon != null) {
                config.iconPath = AssetDatabase.GetAssetPath(primaryGameConfig.properties.icon);
                Log(string.Format("[BuildContainer] IconPath: {0}", config.iconPath));
            } else {
                //config.IconPath = DefaultBuildIcon();
                LogWarning(string.Format("[BuildContainer] Missing Properties.Icon for '{0}' GameConfig", primaryGameConfig.gameID));
            }

            if (primaryGameConfig.properties.splashScreenLogo != null) {
                config.splashScreenLogo = AssetDatabase.GetAssetPath(primaryGameConfig.properties.splashScreenLogo);
                Log(string.Format("[BuildContainer] IconPath: {0}", config.iconPath));
            }

            if (primaryGameConfig.properties.splashScreenLogoBg != null) {
                config.splashScreenLogoBg = AssetDatabase.GetAssetPath(primaryGameConfig.properties.splashScreenLogoBg);
                Log(string.Format("[BuildContainer] IconPath: {0}", config.iconPath));
            }

            config.blurSplashBg = primaryGameConfig.properties.splashBgBlur;

            config.scenes = new List<SceneAssets>();

            config.isDevelopmentBuild = isDevelopmentBuild;
        }
    }
}

#endif