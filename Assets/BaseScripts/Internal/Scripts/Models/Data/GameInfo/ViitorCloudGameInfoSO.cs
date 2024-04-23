#if UNITY_EDITOR

using static Modules.Utility.Utility;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.Serialization;

namespace ViitorCloud.Base.BaseScripts.Internal.Scripts.Models.Data.GameInfo {
	// EditorGUI cannot handle default layouts for scriptable objects
	// Keep this storage class separate so we can save/load it
	[CreateAssetMenu(menuName = "ViitorCloud/Build Config/Game Info")]
	public class ViitorCloudGameInfoSo : ScriptableObject, ISerializationCallbackReceiver {
		// Unity has issues finding ScriptableObjects without some sort of concrete data in them
		[FormerlySerializedAs("DummyData")]
		[HideInInspector] public string dummyData = "?";

		[FormerlySerializedAs("Value")]
		public ViitorCloudGameInfo value = new ViitorCloudGameInfo();

#pragma warning disable 618
		public void OnAfterDeserialize() {
			// Can't have logs here (because logs will try to get Time.realtimeSinceStartup), cannot log during deserialization
			// Cleanup data
			value.scenes ??= new List<SceneAssets>();

			//for (int i = 0; i < Value.Scenes.Count; ++i) {
			//	Value.Scenes[i] = CleanWildcardScene(Value.Scenes[i].scene.name);
			//}
		}

		public void OnBeforeSerialize() {
			// migrate to new fields
		
		}

		private string CleanWildcardScene(string scenePath) {
			if (string.IsNullOrWhiteSpace(scenePath)) {
				return scenePath;
			}

			// We can do wildcard searching without the * char, but force it to ensure it's what the dev wanted to do
			if (!scenePath.Contains(("*"))) {
				return scenePath;
			}

			// When doing a file search in unity, we do NOT want any folder paths
			string cleanPath = Path.GetFileName(scenePath);

			// When doing a file search in unity, we do NOT want the extension
			cleanPath = Path.ChangeExtension(cleanPath, "");
			if (cleanPath.EndsWith(".")) {
				cleanPath = cleanPath.Substring(0, cleanPath.Length - 1);
			}

			if (cleanPath != scenePath) {
				LogWarning($"[ViitorCloudGameInfo] With wildcard '*' searches, do not include folder hierarchy or .unity extension");
				return cleanPath;
			}

			return scenePath;
		}

#pragma warning restore 618
	}
}
#endif