using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ValveHandController : MonoBehaviour {
    [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
    [SerializeField] private OneGrabTranslateTransformer ValveGrabTransformer;
    [SerializeField] private float p1;
    [SerializeField] private float p2;
    [SerializeField] private float h;
    [SerializeField] private float value;
    private float previousPos;
    [SerializeField] private bool positionReversed = false;
    [Header("restrictions")]
    public GameObject snapPosition;
    public float snap_open_threshold;
    public bool valveIsOpen;

    public UnityEvent OnValveOpen;
    public UnityEvent OnValveClose;
    //public float plug_open_threshold;

    // Start is called before the first frame update
    void Start() {
        p1 = ValveGrabTransformer.Constraints.MinZ.Value;
        p2 = ValveGrabTransformer.Constraints.MaxZ.Value;
        h = positionReversed ? p1 - p2 : p2 - p1;
        snapPosition.SetActive(false);
    }

    // Update is called once per frame
    void Update() {
        UpdateValve();
    }

    private void UpdateValve() {
        float currentPos = ValveGrabTransformer.transform.localPosition.z;
        if (previousPos == currentPos)
            return;

        previousPos = currentPos;
        value = (((positionReversed ? p1 : p2) - currentPos) / h) * 100;
        _skinnedMeshRenderer.SetBlendShapeWeight(0, value);
        CheckSnapPosition();
    }

    public void ResetValve() {
        Vector3 _resetpos = ValveGrabTransformer.transform.localPosition;
        _resetpos.z = positionReversed?ValveGrabTransformer.Constraints.MinZ.Value:ValveGrabTransformer.Constraints.MaxZ.Value;
        ValveGrabTransformer.transform.localPosition = _resetpos;
    }

    void CheckSnapPosition() {
        valveIsOpen = (value >= snap_open_threshold);
        EnableSnapPosition(valveIsOpen);
    }


    void EnableSnapPosition(bool value) {
        if (snapPosition.activeSelf == value)
            return;

        snapPosition.SetActive(value);

        if (value)
            OnValveOpen.Invoke();
        else
            OnValveClose.Invoke();
    }
}
