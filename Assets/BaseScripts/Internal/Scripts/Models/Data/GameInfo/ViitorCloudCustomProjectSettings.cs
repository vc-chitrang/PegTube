using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Serialization;
namespace ViitorCloud.Base.BaseScripts.Internal.Scripts.Models.Data.GameInfo {
    [Serializable]
    public class ViitorCloudCustomProjectSettings {

        [FormerlySerializedAs("ColorSpace")]
        public ColorSpace colorSpace;

        [FormerlySerializedAs("Tags")]
        [Tooltip("List of all tags your game needs. NOTE: Please prefix all tags with gameid_<tagname> so that they do not conflict with any other potential games in the build")]
        public List<string> tags = null;
        [FormerlySerializedAs("Layers")]
        public List<string> layers = null;

        [FormerlySerializedAs("Physics")]
        [Tooltip("Overrides for the default physics project settings")]
        public ViitorCloudCustomPhysicsSettings physics = null;

        public ViitorCloudCustomProjectSettings() {
            tags = new List<string>();
            layers = new List<string>();
            physics = new ViitorCloudCustomPhysicsSettings();
        }
    }
}
