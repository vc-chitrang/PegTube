using System;
using System.Collections.Generic;

using UnityEngine.Serialization;
namespace ViitorCloud.Base.BaseScripts.Internal.Scripts.Models.Data.Localization {

	[Serializable]
	public class ViitorCloudLocalizationSettings {
		public string[] sheetTitles;
	}

	[Serializable]
	public class ViitorCloudLocalizationEntry {
		[FormerlySerializedAs("Key")]
		public string key;
		[FormerlySerializedAs("Text")]
		public string text;
		public ViitorCloudLocalizationEntry(string newKey, string newText) {
			key = newKey;
			text = newText;
		}
	}

	[Serializable]
	public class ViitorCloudLocalizationData {

		[FormerlySerializedAs("Entries")]
		public List<ViitorCloudLocalizationEntry> entries = new List<ViitorCloudLocalizationEntry>();
	}
}
