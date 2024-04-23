using ViitorCloud;

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using static Modules.Utility.Utility;

namespace ViitorCloud.GameHelper.Util {
    public class ResourceController : IResourceController {
        public class ResourceResultHandle : IResultHandle {

            private UnityEngine.Object result_;
            private Status status_;

            public bool IsDone {
                get {
                    return (status_ == Status.Done || status_ == Status.Failed);
                }
            }

            public Status Status {
                get {
                    return status_;
                }
            }

            public UnityEngine.Object Result {
                get {
                    if (status_ != Status.Done) {
                        Log(string.Format("[ResourceManager] - Cannot access the result value while loading is in progress - returning null"));
                    }
                    if (status_ == Status.Failed) {
                        Log(string.Format("[ResourceManager] - Loading failed - returning null"));
                    }
                    return result_;
                }
            }

            internal void SetResult(UnityEngine.Object _obj) {
                result_ = _obj;
            }

            internal void SetStatus(Status _status) {
                status_ = _status;
            }
        }

        private MonoBehaviour behaviour_;
        private Queue<Action> operationsQueue_;
        private Dictionary<string, ResourceResultHandle> handles_;
        private Coroutine executeAllCoroutine_;

        public IEnumerator ExecuteAllCoroutines() {
            while (operationsQueue_.Count > 0) {
                operationsQueue_.Dequeue().Invoke();
                yield return null;
            }

            executeAllCoroutine_ = null;
        }

        public void Init(MonoBehaviour behaviour) {
            behaviour_ = behaviour;
            operationsQueue_ = new Queue<Action>();
            handles_ = new Dictionary<string, ResourceResultHandle>();
        }

        public IResultHandle Load(string id) {
            var handle = new ResourceResultHandle();

            handle.SetStatus(Status.InProgress);

            var result = LoadInternal(id);

            handle.SetResult(result);
            handle.SetStatus(result != null ? Status.Done : Status.Failed);

            return handle;
        }

        public IResultHandle LoadAsync(string id) {
            var handle = new ResourceResultHandle();

            handle.SetStatus(Status.InProgress);
            handles_.Add(id, handle);

            operationsQueue_.Enqueue(() => {
                behaviour_.StartCoroutine(LoadAsyncInternal(id));
            });

            TryStartQueue();

            return handle;
        }

        private void TryStartQueue() {
            if (executeAllCoroutine_ == null) {
                executeAllCoroutine_ = behaviour_.StartCoroutine(ExecuteAllCoroutines());
            }
        }

        public IEnumerator LoadAsyncInternal(string id) {
            var resourceHandle = Resources.LoadAsync(id);

            while (!resourceHandle.isDone) {
                yield return null;
            }

            ResourceResultHandle handle;

            if (handles_.TryGetValue(id, out handle)) {
                handle.SetResult(resourceHandle.asset);
                handle.SetStatus(Status.Done);
                handles_.Remove(id);
            }
        }

        public UnityEngine.Object LoadInternal(string id) {
            return Resources.Load(id);
        }

        public void Unload(UnityEngine.Object obj) {
            UnloadInternal(obj);
        }

        public void UnloadAsync(string id) {
            throw new NotImplementedException();
        }

        public IEnumerator UnloadAsyncInternal(string id) {
            throw new NotImplementedException();
        }

        public void UnloadInternal(UnityEngine.Object obj) {
            Resources.UnloadAsset(obj);
        }
    }
}