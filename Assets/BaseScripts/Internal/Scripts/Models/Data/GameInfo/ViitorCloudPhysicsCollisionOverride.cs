using System;

using UnityEngine.Serialization;

using ViitorCloud.Base.BaseScripts.BuildUtils;
namespace ViitorCloud.Base.BaseScripts.Internal.Scripts.Models.Data.GameInfo {
    [Serializable]
    public class ViitorCloudPhysicsCollisionOverride {
        [FormerlySerializedAs("Layer1")]
        public ViitorCloudLayer layer1;
        [FormerlySerializedAs("Layer2")]
        public ViitorCloudLayer layer2;
        [FormerlySerializedAs("Ignore")]
        public bool ignore;
    }
}
