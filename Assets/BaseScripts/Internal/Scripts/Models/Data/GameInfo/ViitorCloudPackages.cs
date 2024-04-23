using UnityEngine;
using System;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace ViitorCloud.Container.Internal.Data {
    [Serializable]
    public class ViitorCloudPackages {
        [FormerlySerializedAs("AR")]
        public bool ar;
        [FormerlySerializedAs("VR")]
        public bool vr;
        [FormerlySerializedAs("URP")]
        public bool urp;
        [FormerlySerializedAs("HDRP")]
        public bool hdrp;
        [FormerlySerializedAs("Addressables")]
        public bool addressables;

        [FormerlySerializedAs("DoNotForceChangePipeline")]
        [Header("Editable Settings")]
        public bool doNotForceChangePipeline;
        [FormerlySerializedAs("rendringPipelineAsset")]
        public RenderPipelineAsset renderingPipelineAsset;
    }
}