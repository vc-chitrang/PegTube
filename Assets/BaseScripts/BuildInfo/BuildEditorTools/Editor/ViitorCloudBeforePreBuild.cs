#if UNITY_EDITOR
using System;
namespace ViitorCloud.Base.BaseScripts.BuildInfo.BuildEditorTools.Editor {
	/// <summary>
	/// Valid method signatures:
	/// static void MethodName();
	/// static void MethodName(ViitorCloudBuildConfigObject);
	/// static ViitorCloudBuildConfigObject MethodName(ViitorCloudBuildConfigObject);
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class ViitorCloudBeforePreBuild : Attribute {
		public ViitorCloudBeforePreBuild() { }
	}
}

#endif