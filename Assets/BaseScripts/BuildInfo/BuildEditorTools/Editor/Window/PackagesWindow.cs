#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Modules.Utility.Utility;
using ViitorCloud.Base.BaseScripts.BuildInfo.BuildEditorTools.Editor;
using ViitorCloud.Base.BaseScripts.Internal.Editor.GameConfig;
using ViitorCloud.Base.BaseScripts.Internal.Scripts.Models.Data.GameInfo;

namespace ViitorCloud.Container.Internal.Data {

    public class PackagesWindow : EditorWindow {

        static GameConfigManager gameConfig;

        [MenuItem("ViitorCloud/Packages")]
        public static void ShowWindow() {
            EditorWindow editorWindow = EditorWindow.GetWindow(typeof(PackagesWindow));
            GUIContent gUIContent = new GUIContent();
            gUIContent.text = "Packages";
            editorWindow.titleContent = gUIContent;

            gameConfig = new GameConfigManager();
        }

        void OnGUI() {
            GUILayout.Label("Base Settings", EditorStyles.boldLabel);
            if (gameConfig == null) {
                gameConfig = new GameConfigManager();
            }
            ViitorCloudGameInfo viitorCloudGameInfo = gameConfig.GetGameInfo();

            GUILayout.Label("AR VR", EditorStyles.boldLabel);

            string isARAvailable = viitorCloudGameInfo.packages.ar ? "Uninstall" : "Install";
            if (GUILayout.Button($"{isARAvailable} AR Packages")) {
                if (viitorCloudGameInfo.packages.ar) {
                    PackagesManifestManager.RemoveARPackages();
                } else {
                    PackagesManifestManager.AddARPackages();
                }
                viitorCloudGameInfo.packages.ar = !viitorCloudGameInfo.packages.ar;
            }

            GUILayout.Label("Rendring Pipeline Settings", EditorStyles.boldLabel);

            string isVRAvailable = viitorCloudGameInfo.packages.vr ? "Uninstall" : "Install";
            if (GUILayout.Button($"{isVRAvailable} VR Packages")) {
                if (viitorCloudGameInfo.packages.vr) {
                    PackagesManifestManager.RemoveVRPackages();
                } else {
                    PackagesManifestManager.AddVRPackages();
                }
                viitorCloudGameInfo.packages.vr = !viitorCloudGameInfo.packages.vr;
            }

            string isURPAvailable = viitorCloudGameInfo.packages.urp ? "Uninstall" : "Install";
            if (GUILayout.Button($"{isURPAvailable} URP Packages")) {
                if (viitorCloudGameInfo.packages.urp) {
                    PackagesManifestManager.RemoveUrpPackages();
                } else {
                    SetUpURP();
                    SetupRPAsset(viitorCloudGameInfo);
                }
                viitorCloudGameInfo.packages.urp = !viitorCloudGameInfo.packages.urp;
            }

            string isHDRPAvailable = viitorCloudGameInfo.packages.hdrp ? "Uninstall" : "Install";
            if (GUILayout.Button($"{isHDRPAvailable} HDRP Packages")) {
                if (viitorCloudGameInfo.packages.hdrp) {
                    PackagesManifestManager.RemoveHdrpPackages();
                } else {
                    SetUpHDRP();
                    SetupRPAsset(viitorCloudGameInfo);
                }
                viitorCloudGameInfo.packages.hdrp = !viitorCloudGameInfo.packages.hdrp;
            }

            if (viitorCloudGameInfo.packages.doNotForceChangePipeline) {
                if (GUILayout.Button($"Install Rendring Pipeline Packages Mannualy")) {
                    if (viitorCloudGameInfo.packages.hdrp) {
                        SetUpHDRP();
                        SetupRPAsset(viitorCloudGameInfo);
                    } else if (viitorCloudGameInfo.packages.urp) {
                        SetUpURP();
                        SetupRPAsset(viitorCloudGameInfo);
                    }
                }
            }

            GUILayout.Label("Assetbundle", EditorStyles.boldLabel);

            string isAdrassaBleAvailable = viitorCloudGameInfo.packages.addressables ? "Uninstall" : "Install";
            if (GUILayout.Button($"{isAdrassaBleAvailable} Addrassable Package")) {
                if (viitorCloudGameInfo.packages.addressables) {
                    PackagesManifestManager.RemoveAddressablsPackage();
                } else {
                    PackagesManifestManager.AddAddressablsPackage();
                }
                viitorCloudGameInfo.packages.addressables = !viitorCloudGameInfo.packages.addressables;
            }


        }

        public static void SetUpURP() {
            PackagesManifestManager.AddUrpPackages();
        }

        public static void SetUpHDRP() {
            PackagesManifestManager.AddHdrpPackages();
        }

        public static void SetupRPAsset(ViitorCloudGameInfo viitorCloudGameInfo) {
            if (viitorCloudGameInfo.packages.renderingPipelineAsset != null) {
                GraphicsSettings.renderPipelineAsset = viitorCloudGameInfo.packages.renderingPipelineAsset;
                QualitySettings.renderPipeline = viitorCloudGameInfo.packages.renderingPipelineAsset;
            } else {
                LogError("Project is in URP, but the Render Pipeline has not been assigned in GameInfo.");
            }
        }
    }
}
#endif