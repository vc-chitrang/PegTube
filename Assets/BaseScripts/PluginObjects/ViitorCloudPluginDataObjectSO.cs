#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Serialization;

namespace ViitorCloud.Base.BaseScripts.PluginObjects {
    [CreateAssetMenu(menuName = "ViitorCloud/Plugin/Plugin Info")]
    public class ViitorCloudPluginDataObjectSo : ScriptableObject {
        // Unity has issues finding ScriptableObjects without some sort of concrete data in them
        [FormerlySerializedAs("DummyData")]
        [HideInInspector] public string dummyData = "?";

        [FormerlySerializedAs("Value")]
        public ViitorCloudPluginData value = new ViitorCloudPluginData();
    }
}
#endif
