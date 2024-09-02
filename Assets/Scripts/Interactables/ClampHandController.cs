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
        LockClump(false);
    }

    internal void LockClump(bool isLocked) {
        _isLocked = isLocked;
        _meshRenderer.material = isLocked ? _selectedMaterial : _defaultMaterial;
    }
    public void SelectClumpWithHands() {
        LockClump(!_isLocked);

        if (_isLocked) {
            OnClampClose.Invoke();
        } else {
            OnClampOpen.Invoke();
        }
    }



}
