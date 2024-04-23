#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Threading;

using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

using static Modules.Utility.Utility;

using static System.String;

using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace ViitorCloud.Base.BaseScripts.BuildInfo.BuildEditorTools.Editor {

    public abstract class PackagesManifestManager {
        private static Queue<string> _requeststringsQue = new Queue<string>();

        private static bool _isDownloading = false;
        private static AddRequest _request;

        private static RemoveRequest _removeRequest;

        public static void Add(string packageURL) {
            // Add a package to the project
            if (!IsPackageInstalled(packageURL)) {
                if (!_isDownloading) {
                    _request = Client.Add(packageURL);
                    EditorApplication.update += ProgressAdd;
                } else {
                    _requeststringsQue.Enqueue(packageURL);
                }
            } else {
                Log($"Package : {packageURL} already installed.");
            }
        }

        private static void Remove(string packageURL) {
            // Add a package to the project
            _removeRequest = Client.Remove(packageURL);
            EditorApplication.update += ProgressRemove;
        }

        private static void ProgressAdd() {
            if (_request is not { IsCompleted: true })
                return;
            switch (_request.Status) {
                case StatusCode.Success:
                Log("Installed: " + _request.Result.packageId); //print count
                break;

                case >= StatusCode.Failure:
                LogError(_request.Error.message);
                break;
            }
            if (_requeststringsQue.Count > 0) {
                Thread.Sleep(1000);
                Add(_requeststringsQue.Dequeue());
            }

            EditorApplication.update -= ProgressAdd;
        }

        private static void ProgressRemove() {
            if (!_removeRequest.IsCompleted)
                return;
            switch (_removeRequest.Status) {
                case StatusCode.Success:
                Log("Uninstalled: " + _removeRequest.PackageIdOrName);
                break;

                case >= StatusCode.Failure:
                Log(_removeRequest.Error.message);
                break;
            }
            EditorApplication.update -= ProgressRemove;
        }

        private static bool IsPackageInstalled(string packageId) {
            if (!File.Exists("Packages/manifest.json"))
                return false;

            string jsonText = File.ReadAllText("Packages/manifest.json");
            return jsonText.Contains(packageId);
        }

        public static bool ContainsPackage(PackageCollection packages, string packageId) {
            foreach (PackageInfo package in packages) {
                if (CompareOrdinal(package.name, packageId) == 0)
                    return true;
                int index = 0;
                for (; index < package.dependencies.Length; index++) {
                    DependencyInfo dependencyInfo = package.dependencies[index];
                    if (Compare(dependencyInfo.name, packageId) == 0)
                        return true;
                }
            }
            return false;
        }

        public static void AddAddressablsPackage() {
            Add("com.unity.addressables");
        }

        public static void AddARPackages() {
            Add("com.unity.feature.ar");
        }

        public static void AddVRPackages() {
            Add("com.unity.feature.vr");
        }

        public static void AddUrpPackages() {
            Add("com.unity.render-pipelines.universal");
        }

        public static void AddHdrpPackages() {
            Add("com.unity.render-pipelines.high-definition");
        }

        public static void RemoveARPackages() {
            Remove("com.unity.feature.ar");
        }

        public static void RemoveVRPackages() {
            Remove("com.unity.feature.vr");
        }

        public static void RemoveUrpPackages() {
            Remove("com.unity.render-pipelines.universal");
        }

        public static void RemoveHdrpPackages() {
            Remove("com.unity.render-pipelines.high-definition");
        }

        public static void RemoveAddressablsPackage() {
            Remove("com.unity.addressables");
        }
    }
}

#endif