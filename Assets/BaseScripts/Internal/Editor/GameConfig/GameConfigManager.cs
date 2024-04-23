#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static Modules.Utility.Utility;
using UnityEditor;

using UnityEngine;

using ViitorCloud.Base.BaseScripts.BuildInfo.BuildEditorTools.Editor;
using ViitorCloud.Base.BaseScripts.Internal.Scripts.Models.Data.AssetBundles;
using ViitorCloud.Base.BaseScripts.Internal.Scripts.Models.Data.GameInfo;
using ViitorCloud.Base.BaseScripts.Internal.Scripts.Models.Types;
namespace ViitorCloud.Base.BaseScripts.Internal.Editor.GameConfig {
    public class GameConfigManager {
        // Loaded project values
        public string PrimaryGameId {
            get; private set;
        }
        public string CurrentGameId {
            get; private set;
        }
        public List<string> AvailableGameIds {
            get; private set;
        }
        private List<ViitorCloudGameInfoSo> AvailableGames {
            get;
            set;
        }
        //public ViitorCloudBuildInfoSO BuildInfo { get; private set; }
        //public ViitorCloudBuildInfoSO BuildInfoOverride { get; private set; }
        //public List<ViitorCloudAssetBundleInfoSO> AssetBundleInfos { get; private set; }
        //public ViitorCloudBuildInfoSO CurrentBuildInfo { get { return BuildInfoOverride == null ? BuildInfo : BuildInfoOverride; } }

        private readonly bool _verbose;

        public GameConfigManager(string gameIdToLoad = null, bool verbose = true) {
            _verbose = verbose;
            Reload(gameIdToLoad);
        }

        public void Reload(string gameIdToLoad = null) {
            // Load ScriptableObjects from the project that define our game and build configs
            FindAvailableGames(gameIdToLoad);
            //FindAvailableBuildInfo(gameIdToLoad); 
            //FindAvailableBundleInfo();
            ApplyProjectTags(GetProjectTags());
            ApplyProjectLayers(GetProjectLayers());
            ViitorCloudGameBase.UpdatePhysicsSettings(this);
        }

        public void OnGUISpacer(bool line = true, bool space = true) {
            if (line) {
                EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1f), Color.grey);
            }

            if (space) {
                EditorGUILayout.Space();
            }
        }

        public bool OnGUIValid(out int currentIndex) {
            currentIndex = -1;
            if (AvailableGameIds == null || AvailableGameIds.Count == 0) {
                EditorGUILayout.LabelField("ERROR: Could not find any 'ViitorCloudGameInfoSO' assets. Please use the asset creation menu 'ViitorCloud/Game Info' to create them");
                return false;
            }

            currentIndex = GetCurrentGameIdIndex();
            if (currentIndex >= 0)
                return true;
            EditorGUILayout.LabelField($"ERROR: Could not find '{CurrentGameId}' GameID in loaded game config assets: [{string.Join(",", AvailableGameIds.ToArray())}]");
            return false;

        }

        public bool OnGUIGameToolbar(ref int currentIndex) {
            FindAvailableGames(gameIdToLoad: null);

            GUIContent[] content = new GUIContent[AvailableGameIds.Count];
            for (int i = 0; i < AvailableGameIds.Count; ++i) {
                if (PrimaryGameId == AvailableGameIds[i]) {
                    content[i] = new GUIContent(AvailableGameIds[i] + " (primary)", "Primary GameID");
                } else {
                    content[i] = new GUIContent(AvailableGameIds[i]);
                }
            }

            int selectedGame = GUILayout.Toolbar(currentIndex, content, EditorStyles.boldLabel);
            if (selectedGame != currentIndex || string.IsNullOrEmpty(CurrentGameId)) {
                currentIndex = selectedGame;
                return true;
            }

            return false;
        }

        public bool OnGUIPrimarySelector() {
            EditorGUILayout.BeginHorizontal();

            bool isPrimary = CurrentGameId == PrimaryGameId;

            EditorGUI.BeginDisabledGroup(isPrimary);
            bool newPrimary = EditorGUILayout.ToggleLeft("Primary GameID", isPrimary);

            // We can only enable primary on a non-primary config
            // Can't turn off the current one because that would leave us with no primary config
            bool updatedPrimary = newPrimary && !isPrimary;

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            return updatedPrimary;
        }

        /// <summary>
        /// Find all of the scriptable object assets, should be one per game that define game config data
        /// </summary>
        private void FindAvailableGames(string gameIdToLoad) {
            AvailableGameIds = new List<string>();
            AvailableGames = AssetTool.GetAllInstances<ViitorCloudGameInfoSo>();

            if (AvailableGames.Count == 0) {
                if (_verbose) {
                    LogError("[GameConfigManager] Please create an ViitorCloudGameInfoSO asset per-game in your project. Use the asset creation menu 'ViitorCloud/Game Info' to do so.");
                }

                return;
            }

            // Make sure we're not loading up multiple configs with a single GameID
            HashSet<string> foundGameIds = new HashSet<string>();
            List<int> toRemoveIndices = new List<int>();

            for (int i = 0; i < AvailableGames.Count; ++i) {
                ViitorCloudGameInfoSo game = AvailableGames[i];
                string gameId = game.value.gameID;

                if (!foundGameIds.Add(gameId)) {
                    if (_verbose) {
                        LogError(string.Format("[GameConfigManager] Found duplicate Game Configs for: GameID: {0}, Filename: {1}", gameId, game.name));
                    }

                    toRemoveIndices.Add(i);
                } else {
                    AvailableGameIds.Add(gameId);

                    if (string.IsNullOrEmpty(CurrentGameId) && !string.IsNullOrEmpty(gameId)) {
                        CurrentGameId = gameId;
                    }
                }
            }

            if (!string.IsNullOrEmpty(gameIdToLoad) && foundGameIds.Contains(gameIdToLoad)) {
                CurrentGameId = gameIdToLoad;
            }

            for (int i = toRemoveIndices.Count - 1; i >= 0; --i) {
                AvailableGames.RemoveAt(toRemoveIndices[i]);
            }
            PrimaryGameId = CurrentGameId;

            // Log(string.Format("[GameConfigManager] AvailableGames: {0}", string.Join(",", AvailableGames.Select(x => x.Value.GameID).ToArray()));
            // Log(string.Format("[GameConfigManager] AvailableGameIDs: {0}", string.Join(",", AvailableGameIds.ToArray()));
        }

        /// <summary>
        /// Find the AssetBundleInfo, it can exist anywhere
        /// </summary>


        /// <summary>
        /// Find the BuildInfo (points to the 'primary' gameid). Should only find one per project
        /// </summary>


        public ViitorCloudGameInfo UpdateCurrentGameId(string gameId) {
            if (string.IsNullOrEmpty(gameId)) {
                LogError("[GameConfigManager] Can't set null current game id");
                return null;
            }

            CurrentGameId = gameId;

            return LoadGameConfigInfo(CurrentGameId);
        }


        private int GetCurrentGameIdIndex() {
            return GetGameIdIndex(CurrentGameId);
        }

        private int GetGameIdIndex(string gameId) {
            if (string.IsNullOrEmpty(gameId) || AvailableGameIds == null) {
                return -1;
            }

            return AvailableGameIds.FindIndex(0, x => x == gameId);
        }

        public ViitorCloudGameInfo GetGameInfo() {
            return GetGameInfo(CurrentGameId);
        }
        private ViitorCloudGameInfo GetGameInfo(string gameId) {
            ViitorCloudGameInfoSo gameInfo = GetGameInfoSo(gameId);
            if (gameInfo == null) {
                string availableGames = string.Join(",", AvailableGames.Select(x => x.name));
                Log("[GameConfigManager] - GetGameInfo - Cannot find GameInfo for : ${gameId}. Available Games are: [${availableGames}]");
                return null;
            }

            return gameInfo.value;
        }

        public ViitorCloudGameInfoSo GetGameInfoSo(string gameId) {
            return AvailableGames.Find((x) => x.value.gameID == gameId);
        }

        private ViitorCloudGameInfoSo GetGameConfigSo(string gameId) {
            // GameConfigInfo should come from loaded assets
            FindAvailableGames(gameIdToLoad: null);

            return AvailableGames.FirstOrDefault(game => game.value.gameID == gameId);

        }

        public ViitorCloudGameInfo LoadGameConfigInfo(string gameId, bool create = true) {
            ViitorCloudGameInfoSo gameConfigSo = GetGameConfigSo(gameId);

            if (gameConfigSo != null) {
                return gameConfigSo.value;
            }

            // LogWarningFormat("[GameConfigManager] Headless GameConfig: {0}", gameId);
            return create ? new ViitorCloudGameInfo() : null;

        }

        public string GetGamePath(string gameId, bool assetPath) {
            if (string.IsNullOrEmpty(gameId)) {
                return null;
            }

            ViitorCloudGameInfoSo gameConfigSo = GetGameConfigSo(gameId);

            return gameConfigSo != null ? GetGamePath(gameConfigSo, assetPath) : null;

        }

        // if assetPath is false returns full data path
        // Make sure we are consistently using unix style forward slashes and not mixing in windows pathing
        // Windows is okay with both paths, but unity scenes are referenced by string equality
        private static string GetGamePath(ViitorCloudGameInfoSo gameConfigObj, bool assetPath) {
            if (gameConfigObj == null) {
                return null;
            }

            string gameInfoPath = AssetDatabase.GetAssetPath(gameConfigObj);
            if (assetPath) {
                return Path.GetDirectoryName(gameInfoPath)?.Replace("\\", "/");
            }

            string fullInfoPath = Path.GetFullPath(gameInfoPath);
            return Path.GetDirectoryName(fullInfoPath)?.Replace("\\", "/");
        }

        public ViitorCloudGameLocalizationSetting[] GetLocalizationSettings(ViitorCloudGameInfoSo gameConfigObj) {
            return GetLocalizationSettings(gameConfigObj == null ? null : gameConfigObj.value);
        }

        private ViitorCloudGameLocalizationSetting[] GetLocalizationSettings(ViitorCloudGameInfo gameConfigObj) {
            if (gameConfigObj == null) {
                return null;
            }

            List<ViitorCloudGameLocalizationSetting> result = new List<ViitorCloudGameLocalizationSetting>(GetGameInfoLocalizationSettings(gameConfigObj));

            return result.ToArray();
        }

        public string GetPath(ViitorCloudGameInfoSo gameConfigObj, bool assetPath) {
            return gameConfigObj == null ? null : "";

        }

        public static bool IsValidGameId(string gameId, out string err) {
            err = null;
            if (string.IsNullOrEmpty(gameId)) {
                err = "GameID cannot be empty";
                return false;
            }

            if (gameId.Length < 3) {
                err = "GameID must be at least 3 characters";
                return false;
            }

            if (gameId.Contains(" ")) {
                err = "GameID must not contain spaces";
                return false;
            }
            const string validGameIDRegex = "^[_a-zA-Z0-9\\-]+$";
            Regex rex = new Regex(validGameIDRegex, RegexOptions.Singleline);
            MatchCollection matches = rex.Matches(gameId);

            if (matches.Count != 0)
                return true;
            err = "GameID must match regex: " + validGameIDRegex;
            return false;

        }

        public string GetLocalizationFolder(ViitorCloudGameInfo gameInfo) {
            //if (string.IsNullOrEmpty(gameInfo.LocalizationFolder)) {
            //	return null;
            //}

            //string gamePath = GetGamePath(gameInfo.GameID, assetPath: true);
            //return Path.Combine(gamePath, gameInfo.LocalizationFolder);
            return null;
        }

        public ViitorCloudGameLocalizationSetting[] GetGameInfoLocalizationSettings(ViitorCloudGameInfo gameInfo) {
            //if ((gameInfo.LocalizationSettings == null) || (gameInfo.LocalizationSettings.Length == 0)) {
            //	return null;
            //}

            //List<ViitorCloudGameLocalizationSetting> result = new List<ViitorCloudGameLocalizationSetting>();
            //foreach(ViitorCloudGameLocalizationSetting localizationSetting in gameInfo.LocalizationSettings) {
            //	result.Add(localizationSetting);
            //}

            //if (result.Count == 0) {
            //	return null;
            //}

            //return result.ToArray();
            return null;
        }

        public string GetGlobalAssetsToCopyFolder(ViitorCloudGameInfo gameInfo) {
            //if (string.IsNullOrEmpty(gameInfo.AssetsToCopyGlobal)) {
            //	return null;
            //}

            //string gamePath = GetGamePath(gameInfo.GameID, assetPath: true);
            //return Path.Combine(gamePath, gameInfo.AssetsToCopyGlobal);
            return null;
        }

        public string GetGameAssetsToCopyFolder(ViitorCloudGameInfo gameInfo) {
            //if (string.IsNullOrEmpty(gameInfo.AssetsToCopyGame)) {
            //	return null;
            //}

            //string gamePath = GetGamePath(gameInfo.GameID, assetPath: true);
            //return Path.Combine(gamePath, gameInfo.AssetsToCopyGame);
            return null;
        }

        public string GetPluginsToCopyFolder(ViitorCloudGameInfo gameInfo) {
            //if (string.IsNullOrEmpty(gameInfo.CopyToPlugins)) {
            //	return null;
            //}

            //string gamePath = GetGamePath(gameInfo.GameID, assetPath: true);
            //return Path.Combine(gamePath, gameInfo.CopyToPlugins);
            return null;
        }

        private List<string> GetProjectTags() {
            List<string> tags = new List<string>();
            foreach (ViitorCloudGameInfoSo gameInfo in AvailableGames) {
                tags.AddRange(gameInfo.value.projectSettings.tags);
            }
            return tags;
        }
        private List<string> GetProjectLayers() {
            List<string> layers = new List<string>();
            foreach (ViitorCloudGameInfoSo gameInfo in AvailableGames) {
                layers.AddRange(gameInfo.value.projectSettings.layers);
            }
            return layers;
        }


        private static void ApplyProjectTags(List<string> tags) {
            SerializedObject tagManager = TagAndLayerHelper.FindTagManager();
            foreach (string tag in tags) {
                TagAndLayerHelper.AddTag(tag, tagManager, false);
            }

            tagManager.ApplyModifiedProperties();
        }

        private static void ApplyProjectLayers(List<string> layers) {
            SerializedObject layerManager = TagAndLayerHelper.FindTagManager();
            foreach (string layer in layers) {
                TagAndLayerHelper.AddLayer(layer, layerManager, false);
            }

            layerManager.ApplyModifiedProperties();
        }
    }
}

#endif