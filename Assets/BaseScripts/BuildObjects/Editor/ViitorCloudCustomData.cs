using System;
using static Modules.Utility.Utility;
using UnityEditor;

using UnityEngine;

namespace ViitorCloud.Base.BaseScripts.BuildObjects.Editor {
	public static class ViitorCloudCustomData {
		private const string KAssetSavePath = "Assets/Resources/";
		private const string KAssetName = "ViitorCloudCustomData";
		private static string AssetFullPath { get { return System.IO.Path.Combine(KAssetSavePath, KAssetName) + ".asset"; } }

		/// <summary>Assets/shared</summary>
		public static string GitIgnoreFilePath { get { return DataObject.gitIgnoreFilePath; } }
		public static string ShellScriptFilePath { get { return DataObject.shellScriptFilePath; } }

		private static ViitorCloudCustomDataObject _dataObject;
		private static ViitorCloudCustomDataObject DataObject {
			get {
				if (_dataObject == null) {
					_dataObject = Load();
				}

				return _dataObject;
			}
		}

		private static ViitorCloudCustomDataObject Load() {
			ViitorCloudCustomDataObject asset = null;

			// Try to load a saved asset
			Log("[ViitorCloudCustomData] Loading Asset @ {KAssetName}" );
			try {
				asset = Resources.Load<ViitorCloudCustomDataObject>(KAssetName);
			} catch (Exception e) {
                Log(string.Format("[ViitorCloudCustomData] Could not load ViitorCloudCustomDataObject asset at: {0}\n{1}", AssetFullPath, e));
			}

			// Create and save a new asset (save in editor only)
			if (asset != null)
				return asset;
			asset = ScriptableObject.CreateInstance<ViitorCloudCustomDataObject>();
#if UNITY_EDITOR
			System.IO.Directory.CreateDirectory(KAssetSavePath);

			Log($"[ViitorCloudCustomData] CreateAsset @ {AssetFullPath}");
			AssetDatabase.CreateAsset(asset, AssetFullPath);
			AssetDatabase.SaveAssets();
#else
				LogError("[ViitorCloudCustomData] No data to load, online functionality will not work!");
#endif

			return asset;
		}

#if UNITY_EDITOR
		[MenuItem("ViitorCloud/Data/Custom Paths", false, 102)]
		public static void Open() {
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = DataObject;
		}
#endif
	}
}
