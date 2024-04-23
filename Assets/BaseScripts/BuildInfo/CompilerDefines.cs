using System.Collections.Generic;
using System.Linq;

using static Modules.Utility.Utility;

using UnityEngine;

namespace ViitorCloud.Base.BaseScripts.BuildInfo {
    public static class CompileDefines {
        public static string Filename { get { return Application.dataPath + "/Resources/CompileDefines.txt"; } }

        public static string GetFlagsString(string separator = ",") {
            List<string> flags = GetFlags();
            if (flags == null || flags.Count == 0) {
                return "";
            }

            return string.Join(separator, flags.ToArray());
        }

        private static List<string> GetFlags() {
            const string qaFlag = "QA";
            const string betaFlag = "BETA";

            List<string> compilerFlags = new List<string>();

            TextAsset asset = (TextAsset)Resources.Load("CompileDefines", typeof(TextAsset));
            if (asset == null || asset.text == null) {
#if !UNITY_EDITOR && RELEASE
				LogError("[Analytics] CompileDefines file is missing! Build the project w/ UCB or through the build menu to generate it.");
#endif
            } else {
                Log($"[Analytics] Loaded compiler defines: {asset.text}");

                string[] compilerFlagsArray = asset.text.Split(';');

                // don't add QA/BETA flags here for some basic sanity checking on the compiler flags 
                // as we've multiple times released a non-QA build with the QA flag sent to analytics
                // due to an incorrectly generated text file
                // given how painful it is to get the analytics to the right place if the QA/BETA flag is incorrect
                // let's not rely on the text file for those and add them explicitly based on whether the compile flag is set
                if (compilerFlagsArray != null) {
                    compilerFlags.AddRange(compilerFlagsArray.Where(flag => !string.IsNullOrEmpty(flag) && !flag.Equals(qaFlag) && !flag.Equals(betaFlag)));
                }
            }

            // Manually add compile defines that we know go into the build so we don't rely on the generated .txt file for everything
#if UNITY_EDITOR
            if (!compilerFlags.Contains("UNITY_EDITOR")) {
                compilerFlags.Add("UNITY_EDITOR");
            }
#endif

#if UNITY_IPHONE
		if (!compilerFlags.Contains("UNITY_IPHONE")) {
			compilerFlags.Add("UNITY_IPHONE");
		}
#endif

#if UNITY_IOS
		if (!compilerFlags.Contains("UNITY_IOS")) {
			compilerFlags.Add("UNITY_IOS");
		}
#endif

#if AUTOMATIC_CAPTURES
		if (!compilerFlags.Contains("AUTOMATIC_CAPTURES")) {
			compilerFlags.Add("AUTOMATIC_CAPTURES");
		}
#endif

#if CAPHIST
		if (!compilerFlags.Contains("CAPHIST")) {
			compilerFlags.Add("CAPHIST");
		}
#endif

#if QA
		compilerFlags.Add(qaFlag);
#endif

#if BETA
		compilerFlags.Add(betaFlag);
#endif

            return compilerFlags;
        }
    }
}