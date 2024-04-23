using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using UnityEngine;

using Debug = UnityEngine.Debug;
using Color = UnityEngine.Color;

namespace Modules.Utility {

    public static class Utility {

        public static void Log(string msg, Color color, GameObject go = null) {
            string className = new StackFrame(1).GetMethod().DeclaringType?.Name;
            Debug.Log($"<color={color.ToString()}>[{className}] {msg}</color>", go);
        }

        public static void LogWarning(string msg, Color color, GameObject go = null) {
            string className = new StackFrame(1).GetMethod().DeclaringType?.Name;
            Debug.Log($"<color={color.ToString()}>[{className}] {msg}</color>", go);
        }

        public static void LogWarning(string msg, GameObject game = null) {
            string className = new StackFrame(1).GetMethod().DeclaringType?.Name;
            Log($"[{className}] {msg}", Color.yellow, game);
        }

        public static void Log(string msg, GameObject game = null) {
            string className = new StackFrame(1).GetMethod().DeclaringType?.Name;
            Debug.Log($"[{className}] {msg}", game);
        }

        public static void LogError(string msg, GameObject game = null) {
            string className = new StackFrame(1).GetMethod().DeclaringType?.Name;
            Debug.LogError($"[{className}] {msg}", game);
        }

        public static bool IsNull<T>(T source) where T : struct {
            return source.Equals(default(T));
        }

        public static Sprite LoadSprite(string path) {
            if (string.IsNullOrEmpty(path))
                return null;
            if (!File.Exists(path))
                return null;
            Texture2D texture = LoadTexture(path);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            return sprite;
        }

        public static Texture2D LoadTexture(string path) {
            if (string.IsNullOrEmpty(path))
                return null;
            if (!File.Exists(path))
                return null;
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            return texture;
            //return ResizeTexture(path);
        }

        public static Sprite ConvertToSpriteFromTexture2D(Texture2D texture) {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }

        public static Texture2D ConvertToTexture2DFromRenderTexture(RenderTexture rTex) {
            Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
            // ReadPixels looks at the active RenderTexture.
            RenderTexture.active = rTex;
            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// This is Utility method to Reverse animation ONLY WORKS ON LEGACY ANIMATIONS
        /// </summary>
        /// <param name="monoBehaviour">Where this Coroutine will run</param>
        /// <param name="anim">Animation which will be reverced/forward</param>
        /// <param name="reverse">Should this animation play in reverse</param>
        /// <param name="waitTime">How much time in seconds after animation should play</param>
        /// <param name="initWaitTime"></param>
        /// <param name="turnOfObject">should the animation object to be disable/enable after animation done playing</param>
        /// <param name="turnOnImmediately">Should the animation object turned on before waitTime</param>
        /// <param name="animationDoneCallback">Callback when animation is done</param>
        /// <param name="clipIndex"></param>
        public static void AnimationReversal(MonoBehaviour monoBehaviour, Animation anim, bool reverse, float waitTime, float initWaitTime, bool turnOfObject, bool turnOnImmediately, Action animationDoneCallback = null, string clipIndex = "#") {
            monoBehaviour.StartCoroutine(Enumerator());
            return;

            IEnumerator Enumerator() {
                yield return new WaitForSecondsRealtime(initWaitTime);
                if (anim.isPlaying) {
                    LogError("Animation is already playing : " + anim.name, anim.gameObject);
                    yield return null;
                }

                if (turnOnImmediately) {
                    anim.gameObject.SetActive(true);
                }
                yield return new WaitForSecondsRealtime(waitTime);

                string animationName = clipIndex == "#" ? anim.clip.name : clipIndex;
                if (reverse) {
                    // Reverse animation play
                    anim[animationName].speed = -1;
                    anim[animationName].time = anim[animationName].length;
                    anim.Play(animationName);
                } else {
                    anim[animationName].speed = 1;
                    anim[animationName].time = 0;
                    anim.Play(animationName);
                }
                if (turnOfObject) {
                    yield return new WaitForSecondsRealtime(anim[animationName].length);
                    anim.gameObject.SetActive(false);
                }
                yield return new WaitForSecondsRealtime(anim[animationName].length);
                animationDoneCallback?.Invoke();
            }
        }
        static AnimationClip GetClipByIndex(int index, Animation animation) {
            int i = 0;
            foreach (AnimationState animationState in animation) {
                if (i == index) {
                    return animationState.clip;
                }
                i++;
            }
            return null;
        }

        public static Texture2D SpriteToTexture2D(Sprite sprite) {
            Texture2D croppedTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            Color[] pixels = sprite.texture.GetPixels((int)sprite.rect.x,
                (int)sprite.rect.y, (int)sprite.rect.width,
                (int)sprite.rect.height);
            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();
            return croppedTexture;
        }

        public static T Clone<T>(object o) {
            string clonedJson = JsonUtility.ToJson(o);
            return JsonUtility.FromJson<T>(clonedJson);
        }
        public static List<TEnum> GetEnumList<TEnum>() where TEnum : Enum => ((TEnum[])Enum.GetValues(typeof(TEnum))).ToList();
    }
}