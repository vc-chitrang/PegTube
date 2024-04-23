#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;
using UnityEngine.Serialization;

using ViitorCloud.Container.Internal.Data;
namespace ViitorCloud.Base.BaseScripts.Internal.Scripts.Models.Data.GameInfo {
	public enum ReflectorOverlayPolicy {
		AlwaysShow = 0,
		HideWhenNoVisionRequired = 1,
	}

	[Serializable]
	public class ViitorCloudGameLocalizationSetting {
		[FormerlySerializedAs("LocalizationURL")]
		[Tooltip("URL to the HTML version of your localization google sheet. Add the File -> Publish to the Web and choose Entire Document + Web Page. Make sure 'Automatically republish' is enabled.")]
		public string localizationURL = "";
		public ViitorCloudGameLocalizationSetting(string newURL = "") {
			localizationURL = newURL;
		}
		public static ViitorCloudGameLocalizationSetting DefaultSetting = new ViitorCloudGameLocalizationSetting();
	}

	[Serializable]
	public class ViitorCloudGameInfo {

		/// <summary> Set at build time before class is serialized to json. Path found by looking for ViitorCloudGameInfoSO object location </summary>
		[FormerlySerializedAs("GamePath")]
		[HideInInspector] public string gamePath;

		/// <summary> Set at build time before class is serialized to json. Set if we find the localization asset in a folder </summary>
		[FormerlySerializedAs("UsesLocalization")]
		[HideInInspector] public bool usesLocalization;


		// -----------------------------------------------------------

		[FormerlySerializedAs("GameID")]
		[Tooltip("Unique Identifier for your game. Include only alphanumeric characters")]
		public string gameID;

		[Tooltip("Repository URL for this Game")]
		public string repoPath;

		[FormerlySerializedAs("Scenes")]
		[Header("GAME PATHS")]
		[Tooltip("Paths to the scenes you want loaded on startup. 1st scene will be startup scene. Scenes will be added to project in dummy loader, while asset bundled in the full built app. Relative to your game folder, add the folder name and .unity extension. NOTE: You can also use a wildcard search with the '*' character. If you do this, the scene does NOT have to be in your hierarchy. If you do this, do NOT include the folder name in the string and do NOT include .unity extension")]
		//public List<string> Scenes = new List<string>() { "Scenes/main.unity" };
		public List<SceneAssets> scenes = new List<SceneAssets>();

		[FormerlySerializedAs("PhotonID")]
		[Tooltip("If your game uses Photon then enter the photonID here else keep it empty")]
		public string photonID;

		[FormerlySerializedAs("AgoraID")]
		[Tooltip("If your game uses Agora then enter the photonID here else keep it empty")]
		public string agoraID;

		[FormerlySerializedAs("ProjectSettings")]
		[Header("PROJECT SETTINGS")]
		public ViitorCloudCustomProjectSettings projectSettings = new ViitorCloudCustomProjectSettings();

		[FormerlySerializedAs("Properties")]
		[Header("BUILD PROPERTIES")]
		public ViitorCloudGameBuildProperties properties = new ViitorCloudGameBuildProperties();

		[FormerlySerializedAs("Packages")]
		[Header("Built-In Packages Settings")]
		public ViitorCloudPackages packages = new ViitorCloudPackages();

		// -------------------------------------------
		// Helper Functions

		public void RuntimeClean() {
			// TODO: We should not be loading the build settings from the same file as game stuff
			if (properties != null) {
				// Make sure we are not leaving a giant texture in memory at runtime
				properties.icon = null;
			}
		}

		
		public string GameSubPath(string path, bool stripAssetsFolder = false) {
			return CleanScenePath(System.IO.Path.Combine(gamePath, path), stripAssetsFolder);
		}

		public static string CleanScenePath(string path, bool stripAssetsFolder) {
			path = path.Replace("\\", "/");

			if (stripAssetsFolder && path.StartsWith("Assets/")) {
				path = path.Substring(7);
			}

			if (path.Contains("../")) {
				List<string> parts = path.Split('/').ToList();
				int index = parts.FindIndex(p => p == "..");
				while (index >= 0) {
					parts.RemoveAt(index);
					if (index > 1) {
						parts.RemoveAt(index - 1);
					}
					index = parts.FindIndex(p => p == "..");
				}

				path = string.Join("/", parts.ToArray());
			}

			return path;
		}
	
	}

	[Serializable]
	public class SceneAssets {
		public bool active;
		public SceneAsset scene;
    }
}
#endif