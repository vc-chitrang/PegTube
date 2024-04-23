#if UNITY_EDITOR
using static Modules.Utility.Utility;
namespace ViitorCloud.Base.BaseScripts.BuildInfo.BuildEditorTools.Editor {
	public static class SafeAssetDatabase {
		public static void Refresh() {
			if (UnityEditor.EditorApplication.isUpdating) {
				LogWarning("[SafeAssetDatabase] Skipping asset refresh, editor is updating");
				return;
			}

			UnityEditor.AssetDatabase.Refresh();
		}
	}
}

#endif