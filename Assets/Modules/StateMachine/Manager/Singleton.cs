using ViitorCloud;

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using static Modules.Utility.Utility;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Be aware this will not prevent a non singleton constructor
///   such as `T myT = new T();`
/// To prevent that, add `protected T () {}` to your singleton class.
///
/// As a note, this is made as MonoBehaviour because we need Coroutines.
/// </summary>


namespace ViitorCloud.GameHelper.Util {
    public static class SingletonLoader {
        private static bool initializedSingletons_ = false;

        public static bool InitializingInEditor { get; private set; }

        public static void InitializeSingletonsInEditor() {
            // we can't pass arguments to InitializeSingletons
            InitializingInEditor = true;
            InitializeSingletons();
            InitializingInEditor = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeSingletons() {
            if (initializedSingletons_) {
                return;
            }

            initializedSingletons_ = true;

            Log(string.Format("[Singleton] BEGIN InitializeSingletons({0})", InitializingInEditor ? "editor" : "player"));
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> allTypes = new List<Type>();

            // We do this manually in case any assembly is broken/unloaded by Unity, and would exception out the entire Linq operaiton
            foreach (Assembly asm in assemblies) {
                try {
                    allTypes.AddRange(asm.GetTypes());
                } catch (Exception e) {
                    if (asm.FullName.StartsWith("Stores")) {
                        // Ignore, known error with IAP dlls
                    } else {
                        LogError(string.Format("[Singleton] Could not reflect types from assmebly: {0}\n{1}", asm.FullName, e));
                    }
                }
            }

            foreach (var type in allTypes) {
                if (type.IsAbstract) {
                    continue;
                }

                Type typeIter = type;
                while (typeIter != null) {
                    typeIter = typeIter.BaseType;
                    if (typeIter == null) {
                        break;
                    }

                    if (!typeIter.IsGenericType) {
                        continue;
                    }

                    if (typeIter.GetGenericTypeDefinition() != typeof(Singleton<>)) {
                        continue;
                    }

                    var methodInfo = typeIter.GetMethod("UnforcedCacheSingletonIfNecessary", BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Static);
                    if (methodInfo != null) {
                        Log($"[SingletonLoader] Loading singleton: {typeIter}");
                        methodInfo.Invoke(null, null);
                        break;
                    }
                }
            }

            Log("[Singleton] END InitializeSingletons()");
        }
    }

    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
        private static T instance_;

        private static object lock_ = new object();

        public static T Instance {
            get {
                return Singleton<T>.instance_;
            }
        }


        public static void ForceLoad() {
            ForcedCacheSingletonIfNecessary();
        }

        private static void UnforcedCacheSingletonIfNecessary() {
            CacheSingletonIfNecessary(force: SingletonLoader.InitializingInEditor);
        }

        private static void ForcedCacheSingletonIfNecessary() {
            CacheSingletonIfNecessary(force: true);
        }

        private static void CacheSingletonIfNecessary(bool force = false) {
            lock (Singleton<T>.lock_) {
                if (Singleton<T>.instance_ == null) {
                    var behaviours = UnityEngine.Object.FindObjectsOfType<T>();
                    if (behaviours.Length >= 1) {
                        Log($"[Singleton] assigning existing singleton from: {behaviours[0].name}");
                        Singleton<T>.instance_ = behaviours[0];

                        if (behaviours.Length > 2) {
                            LogError("[Singleton] Something went really wrong " +
                                     " - there should never be more than 1 singleton!" +
                                     " Reopening the scene might fix it.");
                        }
                    }

                    if (Singleton<T>.instance_ == null && (force || Application.isPlaying)) {
                        GameObject singleton = new GameObject();
                        Singleton<T>.instance_ = singleton.AddComponent<T>();
                        singleton.name = "(singleton) " + typeof(T).ToString();
                        Log($"[Singleton] Instantiating {singleton.name}");

                        GameObject.DontDestroyOnLoad(singleton);
                    }
                }
            }
        }
    }
}