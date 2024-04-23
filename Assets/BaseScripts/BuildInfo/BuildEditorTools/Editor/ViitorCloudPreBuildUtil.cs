#if UNITY_EDITOR
using static Modules.Utility.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

namespace ViitorCloud.Base.BaseScripts.BuildInfo.BuildEditorTools.Editor {
    public static class ViitorCloudPreBuildUtil {
        public static ViitorCloudBuildConfigObject Run(Type attributeType, ViitorCloudBuildConfigObject config) {
            // Loop over all classes and look for methods with this attribute
            Assembly[] assemblies = null;

            try {
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
            } catch (Exception e) {
                LogError(string.Format("[ViitorCloudPreBuildUtil] Could not grab assemblies! {0}", e));
            }

            if (assemblies is { Length: > 0 }) {
                foreach (Assembly asm in assemblies) {
                    ViitorCloudBuildConfigObject newConfig = Run(attributeType, asm, config);
                    if (newConfig != null) {
                        config = newConfig;
                    }
                }
            } else {
                LogError("[ViitorCloudPreBuildUtil] Could not find any assemblies in current domain. That is super bad");
            }

            return config;
        }

        private static ViitorCloudBuildConfigObject Run(Type attributeType, Assembly asm, ViitorCloudBuildConfigObject config) {
            if (asm == null) {
                return null;
            }

            Type[] types = null;

            try {
                types = asm.GetTypes();
            } catch (Exception e) {
                LogError(string.Format("[ViitorCloudPreBuildUtil] Could not find any types in assembly {0}: {1}", asm, e));
            }

            if (types == null || types.Length == 0) {
                return null;
            }

            foreach (Type type in types) {
                ViitorCloudBuildConfigObject newConfig = Run(attributeType, type, config);
                if (newConfig != null) {
                    config = newConfig;
                }
            }

            return config;
        }

        private static ViitorCloudBuildConfigObject Run(Type attributeType, Type type, ViitorCloudBuildConfigObject config) {
            if (type == null) {
                return null;
            }

            MethodInfo[] methods = null;

            try {
                methods = type.GetMethods();
            } catch (Exception e) {
                LogError(string.Format("[ViitorCloudPreBuildUtil] Could not find any methods in type {0}: {1}", type, e));
            }

            if (methods == null || methods.Length == 0) {
                return null;
            }

            foreach (MethodInfo method in methods) {
                ViitorCloudBuildConfigObject newConfig = Run(attributeType, method, config);
                if (newConfig != null) {
                    config = newConfig;
                }
            }

            return config;
        }

        private static ViitorCloudBuildConfigObject Run(Type attributeType, MethodInfo method, ViitorCloudBuildConfigObject config) {
            if (method == null) {
                return null;
            }

            if (method.ReflectedType == null)
                return null;
            string classname = method.ReflectedType.ToString();

            IEnumerable<CustomAttributeData> customAttrs = method.CustomAttributes;
            if (customAttrs == null) {
                return null;
            }

            ViitorCloudBuildConfigObject newConfig = null;
            foreach (CustomAttributeData attrData in customAttrs) {
                if (attrData == null) {
                    continue;
                }

                if (attrData.AttributeType != attributeType) {
                    continue;
                }

                if (method.IsConstructor) {
                    LogError(string.Format("[ViitorCloudPreBuildUtil] PreBuild method must not be a constructor! {0}.{1}", classname, method));
                    continue;
                }

                if (!method.IsStatic) {
                    LogError(string.Format("[ViitorCloudPreBuildUtil] PreBuild method must be static! {0}.{1}", classname, method));
                    continue;
                }

                if (method.IsSpecialName) {
                    LogError(string.Format("[ViitorCloudPreBuildUtil] PreBuild method must not be a specially named method! {0}.{1}", classname, method));
                    continue;
                }

                ParameterInfo[] methodParams = method.GetParameters();
                object returnObj;
                switch (methodParams.Length) {
                    case 0: {
                        Log(string.Format("[ViitorCloudPreBuildUtil] Running {0} with 0 params", method));
                        returnObj = method.Invoke(obj: null, parameters: null);
                        if (returnObj is ViitorCloudBuildConfigObject obj) {
                            if (attributeType == typeof(ViitorCloudAfterPreBuild)) {
                                LogWarning("[ViitorCloudPreBuildUtil] Are you sure you wanted to return an ViitorCloudBuildConfig object? This is the step AFTER ViitorCloud runs its build scripts");
                            }

                            config = newConfig = obj;
                        }

                        continue;
                    }

                    case > 1:
                        LogError(string.Format("[ViitorCloudPreBuildUtil] PreBuild method must have 0 or 1 (ViitorCloudBuildConfigObject) param! {0}.{1}", classname, method));
                        continue;
                }

                if (methodParams[0].ParameterType != typeof(ViitorCloudBuildConfigObject)) {
                    LogError(string.Format("[ViitorCloudPreBuildUtil] PreBuild method must have 0 or 1 (ViitorCloudBuildConfigObject) param! {0}.{1} -- found {2}", classname, method, methodParams[0].ParameterType));
                    continue;
                }

                Log(string.Format("[ViitorCloudPreBuildUtil] Running {0} with 1 param of ViitorCloudBuildConfigObject", method));
                returnObj = method.Invoke(obj: null, parameters: new object[] {
                    config
                });
                if (returnObj is not ViitorCloudBuildConfigObject configObject)
                    continue;
                if (attributeType == typeof(ViitorCloudAfterPreBuild)) {
                    LogWarning("[ViitorCloudPreBuildUtil] Are you sure you wanted to return an ViitorCloudBuildConfig object? This is the step AFTER ViitorCloud runs its build scripts");
                }

                config = newConfig = configObject;
            }

            return newConfig;
        }
    }
}
#endif