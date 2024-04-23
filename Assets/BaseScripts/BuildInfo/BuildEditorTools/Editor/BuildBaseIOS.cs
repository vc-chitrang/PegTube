#if UNITY_EDITOR


using UnityEditor;

using static Modules.Utility.Utility;

#if UNITY_EDITOR_OSX && UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif


namespace ViitorCloud.Base.BaseScripts.BuildInfo.BuildEditorTools.Editor {
	public static class BuildBaseIOS {
		// ----------------------------------------------------------------------
		// Post Build
		// ----------------------------------------------------------------------
#if UNITY_EDITOR && UNITY_IOS
	public static void PListModifications(BuildTarget buildTarget, string pathToBuiltProject) {
		Log(string.Format("[BuildBase] PListModification({0}, {1})", buildTarget, pathToBuiltProject));

		// Get plist
		string plistPath = pathToBuiltProject + "/Info.plist";
		PlistDocument plist = new PlistDocument();
		plist.ReadFromString(File.ReadAllText(plistPath));

		// Get root
		PlistElementDict rootDict = plist.root;

		// background modes
		PlistElementArray schemes = rootDict.CreateArray("LSApplicationQueriesSchemes");

		Log("[BuildBase] PLIST: set requires full screen...");
		rootDict["UIRequiresFullScreen"] = new PlistElementBoolean(true);

        GameConfigManager config = new GameConfigManager();

        Log("[BuildBase] PLIST: set camera usage description");
		rootDict["NSCameraUsageDescription"] = new PlistElementString(config.GetGameInfoSO(config.CurrentGameId).Value.Properties.CameraUsageDescription);

		Log("[BuildBase] PLIST: set camera usage description");
		rootDict["NSPhotoLibraryUsageDescription"] = new PlistElementString(config.GetGameInfoSO(config.CurrentGameId).Value.Properties.PhotoLibraryUsageDescription);

        Log("[BuildBase] PLIST: set encryption export compliance");
		rootDict["ITSAppUsesNonExemptEncryption"] = new PlistElementBoolean(false);

		if (rootDict.values.ContainsKey("UIApplicationExitsOnSuspend")) {
			Log("[BuildBase] PLIST: removing UIApplicationExitsOnSuspend key which has been deprecated by Apple but Unity still appends");
			rootDict.values.Remove("UIApplicationExitsOnSuspend");
		}

		// Write to file
		File.WriteAllText(plistPath, plist.WriteToString());
	}
#endif

		public static void UpdateBuildNumber() {
			string buildNumber = BuildBase.GetBuildNumber();
			if (string.IsNullOrEmpty(buildNumber)) {
				Log("[BuildBaseIOS] Not updating build number, nothing to update with");
				return;
			}

			if (buildNumber.Equals(PlayerSettings.iOS.buildNumber)) {
				Log(string.Format("[BuildBaseIOS] Build numbers match: {0}", buildNumber));
			} else {
				Log(string.Format("[BuildBaseIOS] Updating build number from: {0} to: {1}", PlayerSettings.iOS.buildNumber, buildNumber));
			}

			PlayerSettings.iOS.buildNumber = buildNumber;
		}
	}
}

#endif