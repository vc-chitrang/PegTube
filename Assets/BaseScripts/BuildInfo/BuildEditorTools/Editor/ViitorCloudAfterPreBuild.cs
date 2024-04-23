#if UNITY_EDITOR
using System;
namespace ViitorCloud.Base.BaseScripts.BuildInfo.BuildEditorTools.Editor {
	/// <summary>
	/// Valid method signatures:
	/// static void MethodName();
	/// static void MethodName(ViitorCloudBuildConfigObject);
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class ViitorCloudAfterPreBuild : Attribute {
		public ViitorCloudAfterPreBuild() { }
	}
}

#endif