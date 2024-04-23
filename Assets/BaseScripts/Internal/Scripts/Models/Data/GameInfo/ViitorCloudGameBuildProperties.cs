using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Serialization;
namespace ViitorCloud.Base.BaseScripts.Internal.Scripts.Models.Data.GameInfo {
    [Serializable]
    public class ViitorCloudGameBuildProperties {
        [FormerlySerializedAs("Title")]
        [Header("Build-Time Properties used by the build system for the primary game")]
        [Tooltip("If built as a standalone app, what title of the game")]
        public string title = null;

        [FormerlySerializedAs("CompanyName")]
        public string companyName = "ViitorCloud";

        [FormerlySerializedAs("BundleId")]
        [Tooltip("If built as a standalone app, what is the bundleid")]
        public string bundleId = null;

        [FormerlySerializedAs("Version")]
        [Tooltip("If built as a standalone app, what version should be listed on the app store. Build numbers will be auto-generated")]
        public string version = null;

        [FormerlySerializedAs("VersionCode")]
        public int versionCode = 0;

        [FormerlySerializedAs("Icon")]
        [Tooltip("If built as a standalone app, will be used as app icon. 1024x1024 PNG")]
        public Texture2D icon = null;

        [FormerlySerializedAs("SplashScreenLogo")]
        [Tooltip("If built as a standalone app, will be used as Splash Screen Logo. 1024x1024 PNG")]
        public Sprite splashScreenLogo = null;

        [FormerlySerializedAs("SplashScreenLogoBG")]
        [Tooltip("If built as a standalone app, will be used as Splash Screen Logo BG.")]
        public Sprite splashScreenLogoBg = null;

        [FormerlySerializedAs("SpashBGBlur")]
        [Tooltip("Should BG of the Splashscreen blur.")]
        public bool splashBgBlur = true;

        [FormerlySerializedAs("Portrait")]
        [Header("Screen Orientation")]
        public bool portrait = true;
        [FormerlySerializedAs("PortraitUpsideDown")]
        public bool portraitUpsideDown = true;
        [FormerlySerializedAs("LandscapeLeft")]
        public bool landscapeLeft = false;
        [FormerlySerializedAs("LandscapeRight")]
        public bool landscapeRight = false;

        public string hostForSchemes = "vc";
        public List<string> urlSchemes;

#if UNITY_IOS
		[FormerlySerializedAs("MinIOSVersionSupported")]
        [Tooltip("The minimum iOS version this build supports. Build will take this from the primary gameinfo file, and ignore subgame values")]
		public string minIOSVersionSupported = "9.0";

        [FormerlySerializedAs("CameraUsageDescription")]
        [Tooltip("If your app requires Camara then add description for the same")]
		public string cameraUsageDescription;

        [FormerlySerializedAs("PhotoLibraryUsageDescription")]
        [Tooltip("If your app requires Photos app permision then add description for the same")]
        public string photoLibraryUsageDescription;

#endif

#if UNITY_ANDROID
#endif
    }
}