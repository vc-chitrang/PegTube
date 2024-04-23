#if UNITY_EDITOR
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Serialization;
using static Modules.Utility.Utility;
using ViitorCloud.Base.BaseScripts.BuildInfo.BuildEditorTools.Editor;

namespace ViitorCloud.Base.BaseScripts.BuildObjects.Editor {
    [Serializable]
    public class ViitorCloudBuildDataObject : ScriptableObject {
        // --------------------------------------------------------------------------------------
        // Data
        // --------------------------------------------------------------------------------------
        public string BuildFolderBase { get { return buildFolderBase; } }
        public List<string> Scenes { get { return scenes; } }

        public bool AutoUpdateAndroidVersion { get { return autoUpdateAndroidVersion; } set { autoUpdateAndroidVersion = value; } }
        public bool AutoUpdateIOSBuildNumber { get { return autoUpdateIOSBuildNumber; } set { autoUpdateIOSBuildNumber = value; } }

        public List<ViitorCloudBuildConfigObject> BuildConfigList { get { return buildConfigList; } }

        public Dictionary<ViitorCloudBuildConfigTarget, Dictionary<string, ViitorCloudBuildConfigObject>> BuildConfigDict {
            get {
                if (_buildConfigDict == null || buildConfigList == null || buildConfigList.Count != _buildConfigDict.Count) {
                    _buildConfigDict = LoadBuildConfigs(buildConfigList);
                }

                return _buildConfigDict;
            }
        }

        [FormerlySerializedAs("scenes_")]
        [Header("Paths")][SerializeField] private List<string> scenes = new List<string>();
        [FormerlySerializedAs("buildFolderBase_")]
        [SerializeField] private string buildFolderBase = "Builds/";

        [FormerlySerializedAs("autoUpdateAndroidVersion_")]
        [Header("Parameters")]
        [SerializeField] private bool autoUpdateAndroidVersion = true;
        [FormerlySerializedAs("autoUpdateIOSBuildNumber_")]
        [SerializeField] private bool autoUpdateIOSBuildNumber = true;

        [FormerlySerializedAs("buildConfigList_")]
        [Header("Build Configs")]
        [SerializeField] private List<ViitorCloudBuildConfigObject> buildConfigList;
        [NonSerialized] private Dictionary<ViitorCloudBuildConfigTarget, Dictionary<string, ViitorCloudBuildConfigObject>> _buildConfigDict;


        // --------------------------------------------------------------------------------------
        // ReSharper disable once CommentTypo
        // Data Struction Swizzling
        // --------------------------------------------------------------------------------------
        public void Load() {
            _buildConfigDict = LoadBuildConfigs(buildConfigList);
        }

        private Dictionary<ViitorCloudBuildConfigTarget, Dictionary<string, ViitorCloudBuildConfigObject>> LoadBuildConfigs(List<ViitorCloudBuildConfigObject> buildConfigList) {
            Dictionary<ViitorCloudBuildConfigTarget, Dictionary<string, ViitorCloudBuildConfigObject>> dict = new Dictionary<ViitorCloudBuildConfigTarget, Dictionary<string, ViitorCloudBuildConfigObject>>();
            if (buildConfigList == null) {
                return dict;
            }

            foreach (ViitorCloudBuildConfigObject config in buildConfigList) {
                if (!dict.ContainsKey(config.target)) {
                    dict[config.target] = new Dictionary<string, ViitorCloudBuildConfigObject>();
                }

                Dictionary<string, ViitorCloudBuildConfigObject> targetDict = dict[config.target];
                if (!targetDict.TryAdd(config.name, config)) {
                    LogError(string.Format("[ViitorCloudBuildData] Cannot have two build configs with the same name! {0}, target={1}", config.name, config.target));
                }
            }

            // Add alternate versions of names for easier lookup
            foreach ((ViitorCloudBuildConfigTarget key, Dictionary<string, ViitorCloudBuildConfigObject> targetDict) in dict) {
                if (!key.ToString().StartsWith("Android")) {
                    continue;
                }

                Dictionary<string, ViitorCloudBuildConfigObject> altDict = new Dictionary<string, ViitorCloudBuildConfigObject>();
                foreach (string configName in targetDict.Keys) {
                    string altName = configName.StartsWith("Android") ? configName.Replace("Android", "") : "Android" + configName;
                    if (!altDict.ContainsKey(altName)) {
                        altDict[altName] = targetDict[configName];
                    }
                }

                foreach (var altKvp in altDict) {
                    targetDict.Add(altKvp.Key, altKvp.Value);
                }
            }

            return dict;
        }

        private int FindIndex(ViitorCloudBuildConfigTarget target, string config, List<ViitorCloudBuildConfigObject> configList) {
            for (int i = 0; i < configList.Count; ++i) {
                if (configList[i].name == config && configList[i].target == target) {
                    return i;
                }
            }

            return -1;
        }

        // --------------------------------------------------------------------------------------
        // Data Validation
        // --------------------------------------------------------------------------------------
        private const string KStringOk = "OK";
        private const string KStringEmpty = "ERR: Empty string";
        private const string KStringBadLength = "ERR: String length {0} < {1}";
        private const string KStringBadContains = "ERR: String does not contains {0}";

        private static string StringOk(ref bool success, string value, int minLength = 0, bool allowEmpty = false, string contains = null) {
            if (string.IsNullOrEmpty(value)) {
                if (allowEmpty) {
                    return KStringOk;
                } else {
                    success = false;
                    return KStringEmpty;
                }
            }

            if (minLength > 0 && value.Length < minLength) {
                success = false;
                return string.Format(KStringBadLength, value.Length, minLength);
            }

            if (!string.IsNullOrEmpty(contains) && !value.Contains(contains)) {
                success = false;
                return string.Format(KStringBadContains, contains);
            }

            return KStringOk;
        }

        public override string ToString() {
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            bool success = true;

            string scenesListOk = scenes is { Count: > 0 } ? "Scenes List OK" : "Nothing in Scenes List";
            foreach (string scene in scenes) {
                scenesListOk += ' ';
                scenesListOk += StringOk(ref success, scene, minLength: 3, allowEmpty: false, contains: ".unity");
            }

            string buildFolderOk = StringOk(ref success, buildFolderBase, minLength: 3, allowEmpty: false);

            s.AppendFormat("[ViitorCloudCustomData] {0}\n", success ? "Looks good" : "ERR: Looks like there was an issue in values (below)");

            s.AppendFormat(" - Scenes : {0}\n", scenesListOk);
            s.AppendFormat(" - Build Folder : {0}\n", buildFolderOk);

            return s.ToString();
        }
    }
}
#endif
