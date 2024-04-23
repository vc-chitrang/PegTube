using System;

using UnityEngine;
using UnityEngine.Serialization;

namespace ViitorCloud.Base.BaseScripts.BuildObjects.Editor {
	[Serializable]
	public class ViitorCloudCustomDataObject : ScriptableObject {
		// --------------------------------------------------------------------------------------
		// Data
		// --------------------------------------------------------------------------------------
		[FormerlySerializedAs("GitIgnoreFilePath")]
		public string gitIgnoreFilePath;
		[FormerlySerializedAs("ShellScriptFilePath")]
		public string shellScriptFilePath;
        // --------------------------------------------------------------------------------------
        // Data Validation
        // --------------------------------------------------------------------------------------
        private const string KStringOk = "OK";
		private const string KStringEmpty = "ERR: Empty string";
		private const string KStringBadLength = "ERR: String length {0} < {1}";
		private const string KStringBadContains = "ERR: String does not contains {0}";
		private const string KStringBadNoAssets = "ERR: String does not start with 'Assets/'";

     

        private static string StringOk(ref bool success, string value, int minLength = 0, bool allowEmpty = false, string contains = null) {
	        if (string.IsNullOrEmpty(value)) {
		        if (allowEmpty) {
			        return KStringOk;
		        } else {
			        success = false;
			        return KStringEmpty;
		        }
	        }

	        if (minLength > 0 && value.Length < minLength) {
		        success = false;
		        return string.Format(KStringBadLength, value.Length, minLength);
	        }

	        if (!string.IsNullOrEmpty(contains) && !value.Contains(contains)) {
		        success = false;
		        return string.Format(KStringBadContains, contains);
	        }

	        return KStringOk;
        }

        public string AssetsStringOk(ref bool success, string value, int minLength = 0, bool allowEmpty = false, string contains = null) {
			bool basicSuccess = true;
			string ok = StringOk(ref basicSuccess, value, minLength, allowEmpty, contains);
			success = basicSuccess;

			if (!success) {
				return ok;
			}

			if (string.IsNullOrEmpty(value)) {
				// We have already checked allowEmpty, just return
				return ok;
			}

			// Must not only contain, must start with this
			if (value.StartsWith("Assets/"))
				return KStringOk;
			success = false;
			return KStringBadNoAssets;
        }
	}
}