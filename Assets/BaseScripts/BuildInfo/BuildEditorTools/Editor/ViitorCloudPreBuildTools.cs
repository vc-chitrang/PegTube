#if UNITY_EDITOR

using System;
using System.IO;
using static Modules.Utility.Utility;
using UnityEngine;
namespace ViitorCloud.Base.BaseScripts.BuildInfo.BuildEditorTools.Editor {
	public static class ViitorCloudPreBuildTools {
		public static void OverrideFile(string src, string dst) {
			string srcPath = Path.Combine(Application.dataPath, src);
			string dstPath = Path.Combine(Application.dataPath, dst);

			LogWarning("[ViitorCloudPreBuildTools] Override: {srcPath} -> {dstPath}");
			try {
				if (File.Exists(dstPath)) {
					File.Delete(dstPath);
				}

				File.Copy(srcPath, dstPath, overwrite: true);
			} catch (Exception e) {
				LogError(string.Format("[ViitorCloudPreBuildTools] Could not overwrite '{0}' with '{1}': {2}", dstPath, srcPath, e));
			}
		}
	}
}
#endif