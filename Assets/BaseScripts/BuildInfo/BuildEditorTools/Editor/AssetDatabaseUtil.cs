#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEditor;

using ViitorCloud.Base.BaseScripts.Internal.Scripts.Models.Data.GameInfo;

namespace ViitorCloud.Base.BaseScripts.BuildInfo.BuildEditorTools.Editor {
    public class RunAnalysisOnPostProcess : AssetPostprocessor {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
            AssetDatabaseUtil.ClearCachedAssets();
        }
    }

    public static class AssetDatabaseUtil {
        public static void ClearCachedAssets() {
            CachedAssets.Clear();
        }

        public static List<T> AllAssetsOfType<T>() where T : UnityEngine.Object {
            Type type = typeof(T);
            if (CachedAssets.TryGetValue(type, out object value))
                return (List<T>)value;

            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            List<T> assets = guids.Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid))).Where(asset => asset != null).ToList();

            CachedAssets[type] = assets;

            return (List<T>)CachedAssets[type];
        }

        public static string[] FindAssetsMatchingFilename(string filename) {
            string[] guids = AssetDatabase.FindAssets(filename);

            // filter by exact filename
            guids = guids.Where((string guid) => {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                return Path.GetFileNameWithoutExtension(path) == filename;
            }).ToArray();

            return guids;
        }

        public static string[] ExplodePath(string filter) {
            if (filter != null && filter.Length != 0) {
                if (!filter.Contains("*")) {
                    return null;
                }

                string[] guids = AssetDatabase.FindAssets(filter);
                if (guids == null || guids.Length == 0) {
                    return null;
                }

                string[] paths = new string[guids.Length];
                for (int i = 0; i < guids.Length; ++i) {
                    paths[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
                }

                return paths;
            }
            return null;

        }

        public static string GetGamePath(ViitorCloudGameInfoSo gameConfigObj, bool assetPath) {
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

        private static readonly Dictionary<Type, object> CachedAssets = new Dictionary<Type, object>();
    }
}
#endif