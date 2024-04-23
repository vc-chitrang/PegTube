using UnityEngine;

using ViitorCloud.Base.BaseScripts.Internal.Scripts.Models.Data.Localization;

namespace ViitorCloud.Container.Internal.Data {
#if INTERNAL_ViitorCloud
	[CreateAssetMenu(menuName = "ViitorCloud/Build Config/Localization Settings")]
#endif
	public class ViitorCloudLocalizationSettingsSO : ScriptableObject {
		// Unity has issues finding ScriptableObjects without some sort of concrete data in them
		[HideInInspector] public string DummyData = "?";

		public string Source = "?";
		public ViitorCloudLocalizationSettings Value;

		public ViitorCloudLocalizationSettingsSO() { Value = new ViitorCloudLocalizationSettings(); }
	}
}
