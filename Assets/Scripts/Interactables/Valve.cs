using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public class Valve : InteractableObjectBase {
    [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;

    internal bool _isClosed = false;

    private void Start() {
        _isClosed = false;
        CloseValve(_isClosed);
    }

    public void OnHoverEnter(HoverEnterEventArgs args) {
        Debug.Log("OnHoverEnter: " + args);
    }

    public void OnSelectEnter(SelectEnterEventArgs args) {
        XRGrabInteractable valve = (XRGrabInteractable)args.interactableObject;
        
        Valve _valve = null;
        valve.TryGetComponent<Valve>(out _valve);

        if (_valve != null) {
            Debug.Log("OnSelectEnter: " + _valve.name);
            _isClosed = !_isClosed;
            _valve.CloseValve(_isClosed);
        }
    }

    internal void CloseValve(bool isClosed) {
        
    }
}
