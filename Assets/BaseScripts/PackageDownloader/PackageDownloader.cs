#if UNITY_EDITOR

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using static Modules.Utility.Utility;

using UnityEditor;

using UnityEngine;
using UnityEngine.Networking;

namespace ViitorCloud.Base.BaseScripts.PackageDownloader {

    public static class PackageDownloader {
        public static void DownloadAndImportPackage(string packageURL) {
            if (!IsGoogleDriveLink(packageURL)) {
                LogError("Only Google Drive link is supported");
                return;
            }

            if (string.IsNullOrEmpty(packageURL)) {
                LogError("Package URL is empty. Please enter a valid URL.");
                return;
            }
            Log(packageURL);
            string tempFilePath = Path.Combine(Application.persistentDataPath, GetFileNameFromGoogleDriveLink(packageURL));

            if (File.Exists(tempFilePath)) {
                packageURL = tempFilePath;
            }

            UnityWebRequest request = UnityWebRequest.Get(packageURL);
            DownloadHandlerBuffer downloadHandler = new DownloadHandlerBuffer();

            request.downloadHandler = downloadHandler;

            // Display progress bar
            //EditorUtility.DisplayProgressBar("Downloading Package", "Please wait...", request.downloadProgress);
            // Use SendWebRequest instead of Send, as it's a non-blocking call
            try {
                request.SendWebRequest().completed += delegate {
                    packageURL = request.url;
                    Log(packageURL);

                    DownloadHandlerBuffer downloadHandler1 = new DownloadHandlerBuffer();
                    UnityWebRequest request1 = new UnityWebRequest(packageURL);
                    request1.downloadHandler = downloadHandler1;

                    request1.SendWebRequest().completed += delegate {
                        OnCompleted(ref request1, ref downloadHandler1);
                    };
                };
            } catch (System.Exception e) {
                LogError(e.Message);
                EditorUtility.ClearProgressBar();
            }
        }

        private static void OnCompleted(ref UnityWebRequest request, ref DownloadHandlerBuffer downloadHandler) {
            string tempFilePath = Path.Combine(Application.persistentDataPath, GetFileNameFromGoogleDriveLink(request.url));
            if (request.result == UnityWebRequest.Result.Success) {
                if (!File.Exists(tempFilePath)) {
                    // Save the downloaded package as a temporary file
                    File.WriteAllBytes(tempFilePath, downloadHandler.data);
                }

                // Import the package into the project
                AssetDatabase.ImportPackage(tempFilePath, false);
                // Delete the temporary file
                //File.Delete(tempFilePath);

                Log("Package downloaded and imported successfully.");
            } else {
                LogError("Failed to download package. Error: " + request.error);
            }

            EditorUtility.ClearProgressBar();
        }

        private static string GetFileNameFromGoogleDriveLink(string googleDriveLink) {
            // Extract file ID from the link
            int fileIdIndex = googleDriveLink.IndexOf("id=", StringComparison.Ordinal) + 3;
            string fileId = googleDriveLink[fileIdIndex..];

            // Extract file name from the file ID
            int slashIndex = fileId.IndexOf("/", StringComparison.Ordinal);
            if (slashIndex != -1) {
                fileId = fileId.Substring(0, slashIndex);
            }

            return fileId + ".unitypackage";
        }

        private static bool IsGoogleDriveLink(string url) {
            // List of known Google Drive hostnames
            string[] googleDriveHosts = {
                "drive.usercontent.google.com"
            };

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
                return false;
            string urlHost = uri.Host.ToLower();
            return googleDriveHosts.Any(host => host.Equals(urlHost));
        }
    }
}

#endif