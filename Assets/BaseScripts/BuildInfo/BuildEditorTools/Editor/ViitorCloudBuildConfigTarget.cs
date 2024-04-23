#if UNITY_EDITOR
namespace ViitorCloud.Base.BaseScripts.BuildInfo.BuildEditorTools.Editor {
	public enum ViitorCloudBuildConfigTarget {
		//Invalid = -1,
		// ReSharper disable once InconsistentNaming
		iOS = 0,
		Android = 1,
		Windows = 2,
		OSX = 3
		//AndroidSamsung = 4,
	}
}

#endif