using static Modules.Utility.Utility;
using System;
using System.Collections.Generic;
using UnityEngine; 

namespace ViitorCloud.GameHelper.Util {
    public class ResourcesManager : Singleton<ResourcesManager> {
        private Dictionary<ControllerType, IResourceController> controllers_;

        private void Awake() {
            Init();
        }

        private void Init() {

            controllers_ = new Dictionary<ControllerType, IResourceController>();

            var resController = new ResourceController();
            resController.Init(this);
            controllers_.Add(ControllerType.Resources, resController);
        }

        public T Load<T>(string pathId) where T : UnityEngine.Object {
            //if (DeviceQuality.IsLandscape) {
            //    pathId = db_.GetPrefabVariantId(pathId);
            //}

            var controller = TryGetController(ControllerType.Resources);
            if (controller == null) {
                return null;
            }

            var handle = controller.Load(pathId);

            T comp = handle.Result as T;

            if (comp == null) {
                var tempGo = handle.Result as GameObject;
                if (tempGo == null) {
                    LogWarning("handle.Result as GameObject -> null");
                    return null;
                }
                comp = tempGo.GetComponent<T>();
            }

            return comp;
        }

        public UnityEngine.Object Load(string pathId) {
            return Load<UnityEngine.Object>(pathId);
        }

        public IResultHandle LoadAsync(string pathId) {

            //if (DeviceQuality.IsLandscape) {
            //    pathId = db_.GetPrefabVariantId(pathId);
            //}

            var controller = TryGetController(ControllerType.Resources);
            if (controller == null) {
                return null;
            }

            return controller.LoadAsync(pathId);
        }

        public void Unload(UnityEngine.Object obj) {

            var controller = TryGetController(ControllerType.Resources);
            if (controller == null) {
                return;
            }

            controller.Unload(obj);
        }

        public void UnloadAsync(UnityEngine.Object obj) {
            throw new NotImplementedException();
        }

        public void UnloadAsync(string id) {
            throw new NotImplementedException();
        }

        private IResourceController TryGetController(ControllerType type) {
            IResourceController controller = null;
            if (controllers_.TryGetValue(type, out controller)) {
                return controller;
            }

           //LogFormat("[ResourcesManager] - There is no controller of type {0}", type);
            return null;
        }
    }

    public interface IResultHandle {
        bool IsDone { get; }
        Status Status { get; }
        UnityEngine.Object Result { get; }
    }

    public enum Status {
        Pending,
        InProgress,
        Done,
        Failed
    }

    public enum ControllerType {
        Resources,
        AssetBundle,
        Addressables
    }
}
