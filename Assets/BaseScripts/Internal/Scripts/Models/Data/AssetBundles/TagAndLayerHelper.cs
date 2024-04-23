#if UNITY_EDITOR
using UnityEditor;
using static Modules.Utility.Utility;
using UnityEngine;
namespace ViitorCloud.Base.BaseScripts.Internal.Scripts.Models.Data.AssetBundles {
    // https://answers.unity.com/questions/33597/is-it-possible-to-create-a-tag-programmatically.html 
    public abstract class TagAndLayerHelper {
        private const int MaxTags = 10000;
        private const int MaxLayers = 31;

        public static SerializedObject FindTagManager() {
            return new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        }

        public static bool AddTag(string tagName, SerializedObject tagManager = null, bool apply = true) {
            tagManager ??= FindTagManager();

            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            if (tagsProp.arraySize >= MaxTags) {
                Log("No more tags can be added to the Tags property. You have " + tagsProp.arraySize + " tags");
                return false;
            }

            if (PropertyExists(tagsProp, 0, tagsProp.arraySize, tagName))
                return false;
            int index = tagsProp.arraySize;
            tagsProp.InsertArrayElementAtIndex(index);
            SerializedProperty sp = tagsProp.GetArrayElementAtIndex(index);
            sp.stringValue = tagName;

            if (!apply)
                return true;
            tagManager.ApplyModifiedProperties();
            return true;

        }

        public static bool RemoveTag(string tagName, SerializedObject tagManager = null, bool apply = true) {
            tagManager ??= FindTagManager();

            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            if (!PropertyExists(tagsProp, 0, tagsProp.arraySize, tagName))
                return false;

            for (int i = 0, j = tagsProp.arraySize; i < j; i++) {

                SerializedProperty sp = tagsProp.GetArrayElementAtIndex(i);
                if (sp.stringValue != tagName)
                    continue;
                tagsProp.DeleteArrayElementAtIndex(i);

                if (apply) {
                    tagManager.ApplyModifiedProperties();
                }
                return true;

            }

            return false;
        }

        public static bool TagExists(string tagName, SerializedObject tagManager = null, bool apply = true) {
            tagManager ??= FindTagManager();

            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            return PropertyExists(tagsProp, 0, MaxTags, tagName);
        }

        public static bool AddLayer(string layerName, SerializedObject tagManager = null, bool apply = true) {
            tagManager ??= FindTagManager();

            SerializedProperty layersProp = tagManager.FindProperty("layers");
            if (PropertyExists(layersProp, 0, MaxLayers, layerName))
                return false;
            for (int i = 6; i < MaxLayers; i++) {
                SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);
                if (sp.stringValue == "") {
                    sp.stringValue = layerName;

                    if (apply) {
                        tagManager.ApplyModifiedProperties();
                    }
                    return true;
                }
                if (i == MaxLayers)
                    Log("All allowed layers have been filled");
            }

            return false;
        }

        public static bool RemoveLayer(string layerName, SerializedObject tagManager = null, bool apply = true) {
            tagManager ??= FindTagManager();

            SerializedProperty layersProp = tagManager.FindProperty("layers");
            if (!PropertyExists(layersProp, 0, layersProp.arraySize, layerName))
                return false;

            for (int i = 0, j = layersProp.arraySize; i < j; i++) {
                SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);

                if (sp.stringValue != layerName)
                    continue;
                sp.stringValue = "";

                if (apply) {
                    tagManager.ApplyModifiedProperties();
                }
                return true;

            }

            return false;
        }

        public static bool LayerExists(string layerName, SerializedObject tagManager = null, bool apply = true) {
            tagManager ??= FindTagManager();

            SerializedProperty layersProp = tagManager.FindProperty("layers");
            return PropertyExists(layersProp, 0, MaxLayers, layerName);
        }

        private static bool PropertyExists(SerializedProperty property, int start, int end, string value) {
            for (int i = start; i < end; i++) {
                SerializedProperty t = property.GetArrayElementAtIndex(i);
                if (t.stringValue.Equals(value)) {
                    return true;
                }
            }
            return false;
        }
    }
}
#endif