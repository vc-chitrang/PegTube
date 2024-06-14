using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ClampHandController : MonoBehaviour {
    [SerializeField] private MeshRenderer _meshRenderer;

    [SerializeField] private Material _defaultMaterial;
    [SerializeField] private Material _selectedMaterial;

    public UnityEvent OnClampOpen;
    public UnityEvent OnClampClose;

    public bool _isLocked = false;
    void Start() {
        _isLocked = false;
        LockClump(_isLocked);
    }

    internal void LockClump(bool isLocked) {
        _meshRenderer.material = isLocked ? _selectedMaterial : _defaultMaterial;
    }
    public void SelectClumpWithHands() {
        _isLocked = !_isLocked;
        LockClump(_isLocked);

        if (_isLocked) {
            OnClampClose.Invoke();
        } else {
            OnClampOpen.Invoke();
        }
    }

}
