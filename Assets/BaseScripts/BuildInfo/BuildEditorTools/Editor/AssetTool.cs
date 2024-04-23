#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;

using ViitorCloud.Base.BaseScripts.Internal.Editor.GameConfig;
using ViitorCloud.Base.BaseScripts.Internal.Scripts.Models.Data.GameInfo;

using Path = System.IO.Path;
using Object = UnityEngine.Object;

namespace ViitorCloud.Base.BaseScripts.BuildInfo.BuildEditorTools.Editor {
    public static class AssetTool {
        public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static T GetSingleInstance<T>(out string error) where T : ScriptableObject {
            error = $"Could not find any instances of: {typeof(T)}";
            List<T> allInstances = GetAllInstances<T>();

            switch (allInstances.Count) {
                case 0:
                    return null;
                case > 1:
                    error = $"Found too many instances ({allInstances.Count}) of: {typeof(T)}";
                    break;
            }

            // Find the first valid build info that corresponds to our game config files
            foreach (T t in allInstances.Where(t => t != null)) {
                error = null;
                return t;
            }

            return null;
        }
        public static List<T> GetAllInstances<T>() where T : ScriptableObject {
            List<T> assets = new List<T>();
            string typename = typeof(T).Name;
            string[] guids = AssetDatabase.FindAssets("t:" + typename);

            if (guids == null)
                return assets;
            assets.AddRange(from guid in guids select AssetDatabase.GUIDToAssetPath(guid) into path where !string.IsNullOrEmpty(path) select AssetDatabase.LoadAssetAtPath<T>(path) into obj where obj != null && obj != default(Object) select obj);

            return assets;
        }
        public static List<string> GetSubFolders(string folderPath) {
            DirectoryInfo dir = new DirectoryInfo(folderPath);
            DirectoryInfo[] info = dir.GetDirectories("*.*");

            List<string> subfolderNames = new List<string>();
            foreach (DirectoryInfo d in info) {
                subfolderNames.Add(d.Name);
            }
            if (subfolderNames.Contains(".git")) {
                subfolderNames.Remove(".git");
            }
            return subfolderNames;
        }

        public static List<string> GetFileNamesInDirectory(string folderPath, bool includeMetaFiles) {
            DirectoryInfo dir = new DirectoryInfo(folderPath);
            FileInfo[] info = dir.GetFiles("*.*");

            List<string> fileNames = new List<string>();
            foreach (FileInfo f in info) {
                if (IsValidFileToCopy(f.Name, includeMetaFiles)) {
                    fileNames.Add(f.Name);
                }
            }
            return fileNames;
        }

        private static bool IsValidFileToCopy(string fileName, bool includeMetaFiles) {
            string[] names = fileName.Split('.');
            // omit hidden files that start with a . and meta files which are in 3 parts if the include parameter for them isn't set
            if (names.Length < 1)
                return false;
            if (names[^1] == "meta") {
                return includeMetaFiles;
            }

            fileName = names[0];
            return !string.IsNullOrEmpty(fileName);
        }

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs = true) {
            DirectoryInfo directory = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] directories = directory.GetDirectories();

            if (!directory.Exists) {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            if (!Directory.Exists(destDirName)) {
                Directory.CreateDirectory(destDirName);
            }

            // Copy Files
            FileInfo[] files = directory.GetFiles();
            foreach (FileInfo file in files) {
                // don't copy meta files because that will result in duplicate guids
                if (!IsValidFileToCopy(file.Name, false))
                    continue;
                string temporaryPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temporaryPath, true);
            }

            // Copy SubDirectory
            if (!copySubDirs)
                return;
            {
                foreach (DirectoryInfo subDirectories in directories) {
                    string temporaryPath = Path.Combine(destDirName, subDirectories.Name);
                    DirectoryCopy(subDirectories.FullName, temporaryPath, copySubDirs);
                }
            }
        }

        public static void IncludeScenesInBuild(List<SceneAssets> scenePaths) {
            if (scenePaths == null || scenePaths.Count == 0) {
                return;
            }

            List<EditorBuildSettingsScene> updatedScenes = new List<EditorBuildSettingsScene>();
            foreach (SceneAssets t in scenePaths) {
                string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(t.scene));
                EditorBuildSettingsScene editorBuildSettingsScene = new EditorBuildSettingsScene(new GUID(guid), t.active);
                updatedScenes.Add(editorBuildSettingsScene);
            }
            EditorBuildSettings.scenes = updatedScenes.ToArray();
        }

        public static List<string> AllScenePaths() {
            List<string> scenePaths = new List<string>();

            for (int i = 0; i < EditorSceneManager.sceneCountInBuildSettings; ++i) {
                scenePaths.Add(EditorSceneManager.GetSceneByBuildIndex(i).path);
            }

            return scenePaths;
        }

        /// <summary>
        /// if <see langword="true"/> then path will be from "Asset/Games/<RepoName>/" else </Reponame>/ 
        /// </summary>
        /// <param name="pathToPlugin"></param>
        /// <param name="fromRepo"></param>
        /// <returns></returns>
        public static string GetRepoPath(string pathToPlugin, bool fromRepo) {
            string[] pluginPaths = AppendPathArray(pathToPlugin);

            bool assetTriggered = false;

            string path = "";

            for (int k = 0; k < pluginPaths.Length; k++) {
                if (fromRepo) {
                    if (pluginPaths[k] == "Assets") {
                        assetTriggered = true;
                        k += 3;
                    }
                    if (assetTriggered) {
                        path = Path.Combine(path, pluginPaths[k]);
                    }
                } else {
                    path += pluginPaths[k] + "/";
                }

            }
            return path;
        }

        public static string[] AppendPathArray(string path) {
            string pluginPath = Application.dataPath[..(Application.dataPath.LastIndexOf("/", StringComparison.Ordinal) + 1)];
            path = path.Replace("\\", "/");
            pluginPath = Path.Combine(pluginPath, path);
            return pluginPath.Split('/');
        }

        public static string AppendPathString(string path) {
#if UNITY_EDITOR_WIN
            string pluginPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("\\", StringComparison.Ordinal) + 1);
#else
            string pluginPath = Application.dataPath[..(Application.dataPath.LastIndexOf("/", StringComparison.Ordinal) + 1)];
            path = path.Replace("\\", "/");
#endif
            pluginPath = Path.Combine(pluginPath, path);
            return pluginPath;
        }

        public static string GetPluginGitIgnorePath() {
            return Path.Combine(AppendPathString(GetRepoPath()), ".gitIgnore");
        }

        public static string GetRepoPath() {
            GameConfigManager config = new GameConfigManager();
            return "Assets/Games/" +
                   Path.GetFileNameWithoutExtension(config.GetGameInfoSo(config.CurrentGameId).value.repoPath);
        }
    }
}

#endif