using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ViitorCloud.GameHelper.Util {
    public interface IResourceController {
        void Init(MonoBehaviour behaviour);
        IResultHandle Load(string id);
        IResultHandle LoadAsync(string id);
        void Unload(UnityEngine.Object objectToUnload);
        void UnloadAsync(string id);

        UnityEngine.Object LoadInternal(string id);
        IEnumerator LoadAsyncInternal(string id);
        void UnloadInternal(UnityEngine.Object objectToUnload);
        IEnumerator UnloadAsyncInternal(string id);
        IEnumerator ExecuteAllCoroutines();
    }
}
