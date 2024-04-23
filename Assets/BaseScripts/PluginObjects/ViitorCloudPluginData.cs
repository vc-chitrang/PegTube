#if UNITY_EDITOR

using System;
using System.Collections.Generic;

using UnityEngine.Serialization;
namespace ViitorCloud.Base.BaseScripts.PluginObjects {
    [Serializable]
    public class ViitorCloudPluginData {
        public List<PluginInfo> pluginInfos = new List<PluginInfo>();
    }
    [Serializable]
    public class PluginInfo {
        [FormerlySerializedAs("PluginName")]
        public string pluginName;
        [FormerlySerializedAs("PluginPath")]
        public string pluginPath;
        [FormerlySerializedAs("PluginVersion")]
        public string pluginVersion;
        [FormerlySerializedAs("PluginDownloadURL")]
        public string pluginDownloadURL;
        public bool requireParentAsset;
        public bool mainAsset;

        public bool isthisAvailableOnDisk { get; set; }

        public PackageType packageType;

        public enum PackageType {
            Path,
            Git,
            ByName,
            None
        }
    }

}
#endif