using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public class Clump : InteractableObjectBase {
    [SerializeField] private MeshRenderer _meshRenderer;

    [SerializeField] private Material _defaultMaterial;
    [SerializeField] private Material _selectedMaterial;
    
    internal bool _isLocked = false;

    private void Start() {
        _isLocked = false;
        LockClump(_isLocked);
    }

    public void OnHoverEnter(HoverEnterEventArgs args) {
        Debug.Log("OnHoverEnter: " + args);
    }

    public void OnSelectEnter(SelectEnterEventArgs args) {
        XRGrabInteractable clump = (XRGrabInteractable)args.interactableObject;
        
        Clump _clump = null;
        clump.TryGetComponent<Clump>(out _clump);

        if (_clump != null) {
            Debug.Log("OnSelectEnter: " + _clump.name);
            _isLocked = !_isLocked;
            _clump.LockClump(_isLocked);
        }
    }

    internal void LockClump(bool isLocked) {
        _meshRenderer.material = isLocked ? _selectedMaterial : _defaultMaterial;
    }
}

