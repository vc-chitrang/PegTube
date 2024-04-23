using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace Modules.Utility {
    public class ProcessDeepLinkMngr : MonoBehaviour {
        public UnityEvent<string> DeepLinkActivated;
        private void Awake() {
            Application.deepLinkActivated += link => DeepLinkActivated.Invoke(link);
        }
    }
}