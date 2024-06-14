using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyringeHandController : MonoBehaviour {

    public Syringe syringe;
    public GameObject syringeHandleGrab;

    private OneGrabTranslateTransformer syringeGrabTransformer;

    float p1;
    float p2;
    float h;
    void Start() {
        syringeGrabTransformer = syringeHandleGrab.GetComponent<OneGrabTranslateTransformer>();
        p1 = syringeGrabTransformer.Constraints.MaxZ.Value;
        p2 = syringeGrabTransformer.Constraints.MinZ.Value;
        h = p2 - p1;
    }

    void Update() {
        UpdateSyringe();
    }

    void UpdateSyringe() {
        if (isValiPosition()) {
            if (!syringeHandleGrab.activeSelf) {
                syringeHandleGrab.SetActive(true);
            }
            syringe.FillWater(CalculateWater());

        } else {
            if (syringeHandleGrab.activeSelf) {
                syringeHandleGrab.SetActive(false);
            }
        }
    }

    bool isValiPosition() {
        return syringe.IsNozzleDipInsideWaterCup();// || condition for placed on tube
    }

    private float CalculateWater() {

        float currentPos = syringeHandleGrab.transform.localPosition.z;
        return ((currentPos - p1) / h) * 100;
    }

    public void ResetSyringe() {
        Vector3 _resetpos = syringeGrabTransformer.transform.localPosition;
        _resetpos.z = syringeGrabTransformer.Constraints.MinZ.Value;
        syringeGrabTransformer.transform.localPosition = _resetpos;
    }
   
}
