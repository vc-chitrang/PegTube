#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using UnityEditor;

using UnityEngine;

using ViitorCloud.Base.BaseScripts.BuildInfo.BuildEditorTools.Editor;
using ViitorCloud.Base.BaseScripts.BuildObjects.Editor;

using static Modules.Utility.Utility;

namespace ViitorCloud.Base.BaseScripts.PluginObjects.Editor {

    public abstract class CheckforPlugin {
        private static List<ViitorCloudPluginDataObjectSo> _pluginInfoSoList;
        public static bool verbose_;

        [MenuItem("ViitorCloud/Plugin/Check Plugin", isValidateFunction: false, 1)]
        public static void CheckForPlugin() {
            FindAvailablePluginInfo();
            CheckPlugin();
        }

        [MenuItem("ViitorCloud/Plugin/Download Plugin", isValidateFunction: false, 2)]
        private static void DownloadThePlugin() {
            FindAvailablePluginInfo();
            if (_pluginInfoSoList.Count <= 0) {
                LogError("Beesly");
                return;
            }
            PluginInfo pluginInfo = _pluginInfoSoList[0].value.pluginInfos.Find(info => info.mainAsset);
            if (pluginInfo != null) {
                PackageDownloader.PackageDownloader.DownloadAndImportPackage(pluginInfo.pluginDownloadURL);
            } else {
                LogError("Null PluginInfo");
            }
            foreach (PluginInfo plugin in _pluginInfoSoList[0].value.pluginInfos) {
                switch (plugin.packageType) {
                    case PluginInfo.PackageType.Path:
                    break;

                    case PluginInfo.PackageType.Git:
                    case PluginInfo.PackageType.ByName:
                    PackagesManifestManager.Add(plugin.pluginDownloadURL);
                    PackagesManifestManager.Add(plugin.pluginName);
                    break;

                    case PluginInfo.PackageType.None:
                    break;

                    default:
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static void FindAvailablePluginInfo() {
            _pluginInfoSoList = null;
            _pluginInfoSoList = AssetTool.GetAllInstances<ViitorCloudPluginDataObjectSo>();

            if (_pluginInfoSoList.Count == 0) {
                if (verbose_) {
                    LogError("ERROR: Could not find ViitorCloudPluginDataObjectSO asset! Attempting to search for override json");
                }
                return;
            }
        }

        private static void CheckPlugin() {
            foreach (PluginInfo pluginInfo in _pluginInfoSoList.SelectMany(pluginInfoSo => pluginInfoSo.value.pluginInfos)) {
                if (!pluginInfo.pluginPath.StartsWith("Assets")) {
                    LogError(string.Format("ERROR: {pluginInfo.pluginPath} does not starts with 'Assets', try giving path from 'Assets'."));
                    return;
                }

                string lastFolderName = AssetTool.GetRepoPath(pluginInfo.pluginPath, true);
                lastFolderName = lastFolderName.Replace('\\', '/');

                int fileCount;
                try {
                    if (pluginInfo.requireParentAsset) {
                        fileCount = Directory.GetFiles(Path.Combine(Application.dataPath, lastFolderName)).Length;
                    } else {
                        fileCount = Directory.GetFiles(AssetTool.AppendPathString(pluginInfo.pluginPath)).Length;
                    }
                } catch (DirectoryNotFoundException) {
                    LogError(string.Format("ERROR: Folder not found {lastFolderName} at {pluginInfo.pluginPath}"));
                    continue;
                }

                if (fileCount <= 0) {
                    EditorUtility.DisplayDialog("Plugin Error", $"ERROR: {pluginInfo.pluginName} is Missing.\nCheck logs for More", "Okay");
                    LogError(string.Format("ERROR: {pluginInfo.pluginName} is Missing from path {AssetTool.AppendPathString(pluginInfo.pluginPath)}\n Download plugin from {pluginInfo.pluginDownloadURL}"));
                    pluginInfo.isthisAvailableOnDisk = false;
                } else {
                    pluginInfo.isthisAvailableOnDisk = true;
                }
                CreateGitIgnoreFile(AssetTool.GetPluginGitIgnorePath(), lastFolderName, pluginInfo.pluginName);
            }
        }

        private static void CreateGitIgnoreFile(string ignorePath, string textToWrite, string pluginName) {
            if (!File.Exists(ignorePath)) {
                FileStream fileStream = File.Create(ignorePath);
                string defaultGitContent = Resources.Load<TextAsset>("DefaultGitIgnoreFile").text;
                fileStream.Close();
                File.WriteAllText(ignorePath, defaultGitContent);
                LogError(".gitIgnore file not available, Creating new");
            }
            textToWrite += "/";

            string gitFileContent = File.ReadAllText(ignorePath);
            if (!gitFileContent.Contains(textToWrite)) {
                textToWrite = System.Environment.NewLine + textToWrite;
                File.AppendAllText(ignorePath, textToWrite);
                Log($"Added <color=white>{pluginName}</color> to gitIgnore");
            } else {
                Log($"Already exist in gitIgnore  <color=white>{textToWrite}</color>");
            }
        }

        private static void AddPreProcessors(bool lockproject) {
            if (_pluginInfoSoList == null || _pluginInfoSoList.Count == 0) {
                CheckForPlugin();
            }
            List<string> paths = AssetTool.AppendPathArray(AssetTool.GetPluginGitIgnorePath()).ToList(); //Path.Combine(Application.dataPath,AssetTool.GetPluginGitIgnorePath()).Replace('\\','/').Split('/').ToList();

            paths.RemoveAt(paths.Count - 1);
            string pathSTR = "";
            for (int i = 0; i < paths.Count; i++) {
                //Log($"[{nameof(CheckforPlugin)}] : Index {i} {paths[i]}");
                pathSTR += Path.Combine(paths[i]) + "/";
            }
            Log(pathSTR);
            List<string> subfolder = AssetTool.GetSubFolders(pathSTR);

            for (int i = 0; i < _pluginInfoSoList.Count; i++) {
                ViitorCloudPluginDataObjectSo PluginInfoSO = _pluginInfoSoList[i];
                for (int j = 0; j < PluginInfoSO.value.pluginInfos.Count; j++) {
                    PluginInfo PluginInfo = PluginInfoSO.value.pluginInfos[j];
                    string lastFolderName = AssetTool.GetRepoPath(PluginInfo.pluginPath, true).Split('/')[0];

                    for (int k = 0; k < subfolder.Count; k++) {
                        if (subfolder[k] == lastFolderName) {
                            //       Log($"<color=green>[{nameof(CheckforPlugin)}] Matched: {lastFolderName} {subfolder[k]}</color>");
                            subfolder.Remove(AssetTool.AppendPathString(subfolder[k]));
                        }
                    }
                }
            }
            for (int i = 0; i < subfolder.Count; i++) {
                string path = AssetTool.AppendPathString(Path.Combine(AssetTool.GetRepoPath(), subfolder[i]));
                string[] CSPath = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
                Log($"<color=white>[{nameof(CheckforPlugin)}] CSFileDir: {path}</color>");
                for (int h = 0; h < CSPath.Length; h++) {
                    if (lockproject) {
                        LockProject(CSPath[h]);
                    } else {
                        UnLockProject(CSPath[h]);
                    }
                }
            }
            if (!lockproject) {
                RunShellScript();
            }
            AssetDatabase.Refresh();
        }

        [MenuItem("ViitorCloud/Plugin/Lock Project", isValidateFunction: false, 2)]
        private static void LockProject() {
            if (EditorUtility.DisplayDialog("Warning", $"Once you Lock the project then all the scripts (excluding from path mentioned in plugin object) will be edited and once you unlock the project changes will be" +
                                                       $"reseted to last commit. So if you have done any code changes will be discarded.", "Continue", "Cancel")) {
                AddPreProcessors(true);
            }
        }

        [MenuItem("ViitorCloud/Plugin/Unlock Project", isValidateFunction: false, 3)]
        private static void UnLockProject() {
            AddPreProcessors(false);
        }

        private static void LockProject(string CSFilePath) {
            //string CSFilePath = @"D:\Unity Project\Category.cs";

            Log($"<color=green>[{nameof(CheckforPlugin)}] CSFileName: {CSFilePath}</color>");
            string content = File.ReadAllText(CSFilePath);
            //int index = 0;
            //foreach (KeyValuePair<string, bool> item in preProcesserList)
            //{
            //    Log($"<color=green>[{nameof(CheckforPlugin)}] AddingPrePrcessor: {item.Key}</color>");
            //    if (content.Contains(item.Key))
            //    {
            //        Log($"<color=red>[{nameof(CheckforPlugin)}] already added: {item.Key}</color>");
            //        continue;
            //    }
            //    if (index != 0)
            //    {
            //        content = content.Insert(0, item.Key + " && ");
            //    }
            //    else
            //    {
            //        content = content.Insert(0, item.Key + "\n\n");
            //    }
            //    index += 1;
            //}

            if (!content.Contains("#if VC_LOCK_PROJECT")) {
                content = content.Insert(0, "#if VC_LOCK_PROJECT\n");
                content += "\n#endif";
            }

            File.WriteAllText(CSFilePath, content);
            //AddPreProcessorsToPlayerSettings();
        }

        private static void UnLockProject(string CSFilePath) {
            Log($"<color=green>[{nameof(CheckforPlugin)}] UnLockProject CSFileName: {CSFilePath}</color>");
            string content = File.ReadAllText(CSFilePath);

            if (content.Contains("#if VC_LOCK_PROJECT")) {
                content = content.Replace("#if VC_LOCK_PROJECT", "");
                content = content.TrimEnd("#endif".ToCharArray());
            }

            File.WriteAllText(CSFilePath, content);
        }

        //[MenuItem("ViitorCloud/Plugin/Lock Editor _8", isValidateFunction: false, 4)]
        private static void LockUnityEditorProject() {
            string CSFilePath = @"D:\Unity Project\Category.cs";

            string path = @"D:\Unity Project\Client\Map Diya\unityvc-base-project-map-diya\Assets\BaseScripts\BuildInfo\BuildEditorTools\" +
                          "/Editor";
            string[] CSPath = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
            Log($"<color=white>[{nameof(CheckforPlugin)}] CSFileDir: {path}</color>");
            for (int h = 0; h < CSPath.Length; h++) {
                CSFilePath = CSPath[h];
                string content = File.ReadAllText(CSFilePath);

                if (!content.StartsWith("#if UNITY_EDITOR")) {
                    content = content.Insert(0, "#if UNITY_EDITOR\n");
                    content += "\n#endif";
                    Log($"<color=green>[{nameof(CheckforPlugin)}] CSFileName: {CSFilePath}</color>");
                } else {
                    Log($"<color=red>[{nameof(CheckforPlugin)}] CSFileName: {CSFilePath}</color>");
                }

                File.WriteAllText(CSFilePath, content);
            }
        }

        //[MenuItem("ViitorCloud/Plugin/Run script _9", isValidateFunction: false, 3)]
        private static void RunShellScript() {
            string[] args = new string[] {
                AssetTool.GetRepoPath(),
            };
            _ = ShellScript.Execute(Path.Combine(Application.dataPath, ViitorCloudCustomData.ShellScriptFilePath), args, false);
        }

        //static void AddPreProcessorsToPlayerSettings()
        //{
        //    if (preProcesserList == null)
        //    {
        //        return;
        //    }

        //    string preprocessors = "";
        //    int index = 0;
        //    foreach (KeyValuePair<string, bool> item in preProcesserList)
        //    {
        //        if (item.Value == true)
        //        {
        //            if (index != preProcesserList.Count - 1)
        //            {
        //                preprocessors += item.Key + ";";
        //            }
        //            else
        //            {
        //                preprocessors += item.Key;
        //            }
        //        }
        //        index += 1;
        //    }
        //    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, preprocessors);
        //    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, preprocessors);
        //    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, preprocessors);

        //}
        public static string RemoveSpecialCharacters(string str) {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str) {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_') {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}

#endif