#if UNITY_EDITOR

using System;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;
using static Modules.Utility.Utility;
using ViitorCloud.Base.BaseScripts.BuildInfo.BuildEditorTools.Editor;

namespace ViitorCloud.Base.BaseScripts.BuildObjects.Editor {
	public static class ViitorCloudBuildData {
		public static string KiosBuildPrefix = "iOS-";
		public static string KAndroidFireBuildPrefix = "Android-";

		public const string KDebugBuildName = "Debug";
		public const string KReleaseBuildName = "Release";
		public const string KBetaBuildName = "Beta";

		private const string KAssetSavePath = "Assets/Resources/";
		private const string KAssetName = "ViitorCloudBuildData";
		private static string AssetFullPath { get { return System.IO.Path.Combine(KAssetSavePath, KAssetName) + ".asset"; } }


		/// <summary>Builds/</summary>
		public static string BuildFolderBase { get { return DataObject.BuildFolderBase; } }

		/// <summary>Default Scenes</summary>
		public static List<string> DefaultScenes { get { return DataObject.Scenes; } }

		/// <summary>update android build number via iOS version + UCB build num (if available)</summary>
		public static bool AutoUpdateAndroidVersion { get { return DataObject.AutoUpdateAndroidVersion; } set { DataObject.AutoUpdateAndroidVersion = value; } }
		
		/// <summary>update iOS build number via UCB build num (if available)</summary>
		public static bool AutoUpdateIOSVersion { get { return DataObject.AutoUpdateIOSBuildNumber; } set { DataObject.AutoUpdateIOSBuildNumber = value; } }

		/// <summary>config info on build targets</summary>
		private static Dictionary<ViitorCloudBuildConfigTarget, Dictionary<string, ViitorCloudBuildConfigObject>> BuildConfigs { get { return DataObject.BuildConfigDict; } }

		private static bool HasConfig(ViitorCloudBuildConfigTarget target, string name) {
			if (!BuildConfigs[target].ContainsKey(name)) {
				Log(string.Format("[ViitorCloudBuildData] BuildConfigs[{0}] does not contain name: '{1}'", target, name));
				return false;
			}
			
			Log(string.Format("[ViitorCloudBuildData] BuildConfigs[{0}] contains name: '{1}'", target, name));
			return true;
		}

		public static ViitorCloudBuildConfigObject GetConfig(ViitorCloudBuildConfigTarget target, string name, string prefix = null) {
			// Support for the old naming scheme of "Debug" and the new naming scheme of "Fire-Debug"
			// Support for "QA" builds and "Debug" builds as interchangeable
			Log(string.Format("[ViitorCloudBuildData] GetConfig('{0}', '{1}', '{2}')", target.ToString(), name, prefix));

			if (BuildConfigs == null) {
				throw new UnityEditor.Build.BuildFailedException("[ViitorCloud.Shared.Build.UCB] Could not load BuildConfigs");
			}

			if (!BuildConfigs.ContainsKey(target)) {
				throw new UnityEditor.Build.BuildFailedException($"[ViitorCloud.Shared.Build.UCB] BuildConfigs have no definitions for target: {target}");
			}

			int buildConfigCount = 0;
			foreach (string configName in BuildConfigs[target].Keys) {
				Log(string.Format("[ViitorCloudBuildData] Found BuildConfig[{0}] = {1}", ++buildConfigCount, configName));
			}

			string fullName = prefix + name;
			if (HasConfig(target, fullName))
				return BuildConfigs[target][fullName];
			fullName = name;
			if (HasConfig(target, fullName))
				return BuildConfigs[target][fullName];
			name = name switch {
				"QA" => "Debug",
				"Debug" => "QA",
				_ => name
			};

			fullName = prefix + name;
			if (HasConfig(target, fullName))
				return BuildConfigs[target][fullName];
			fullName = name;
			return !HasConfig(target, fullName) ? null : BuildConfigs[target][fullName];
		}

		private static ViitorCloudBuildDataObject _dataObject;
		public static ViitorCloudBuildDataObject DataObject {
			get {
				if (_dataObject == null) {
					_dataObject = Load();
				}

				return _dataObject;
			}
		}

		private static ViitorCloudBuildDataObject Load() {
			ViitorCloudBuildDataObject asset = null;

			// Try to load a saved asset
			Log($"[ViitorCloudBuildData] Loading Asset @ {KAssetName}");
			try {
				asset = Resources.Load<ViitorCloudBuildDataObject>(KAssetName);
			} catch (Exception e) {
				Log(string.Format("[ViitorCloudBuildData] Could not load ViitorCloudBuildDataObject asset at: {0}\n{1}", AssetFullPath, e));
			}

			// Create and save a new asset (save in editor only)
			if (asset == null) {
				asset = ScriptableObject.CreateInstance<ViitorCloudBuildDataObject>();
				asset.Load();
#if UNITY_EDITOR
				System.IO.Directory.CreateDirectory(KAssetSavePath);

				Log(string.Format("[ViitorCloudBuildData] CreateAsset @ {0}", AssetFullPath));
				AssetDatabase.CreateAsset(asset, AssetFullPath);
				AssetDatabase.SaveAssets();
#else
				LogError("[ViitorCloudBuildData] No data to load, UCB Builds and Build Menu will not work");
#endif
			} else {
				asset.Load();
			}

			return asset;
		}

#if UNITY_EDITOR
		[MenuItem("ViitorCloud/Data/Build Configs", false, 102)]
		public static void Open() {
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = DataObject;
		}
#endif
	}
}
#endif
