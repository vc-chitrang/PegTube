using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyringeHandController : MonoBehaviour {

    public Syringe syringe;
    public GameObject syringeHandleGrab;

    public GameObject UpArrow;
    public GameObject DownArrow;

    private OneGrabTranslateTransformer syringeGrabTransformer;
    private FlowController _flowController;

    float p1;
    float p2;
    float h;
    void Start() {
        _flowController = FindAnyObjectByType<FlowController>();
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
            float waterAmount = CalculateWater();
            syringe.FillWater(waterAmount);
            EnableArrow(waterAmount);
        } else {
            if (syringeHandleGrab.activeSelf) {
                syringeHandleGrab.SetActive(false);
            }
            if (UpArrow.activeSelf) {
                UpArrow.SetActive(false);
            }
            if (DownArrow.activeSelf) {
                DownArrow.SetActive(false);
            }
        }
    }

    private void EnableArrow(float waterAmount) {// 0 to 100
        if (syringe.IsNozzleDipInsideWaterCup() && waterAmount < 10 && _flowController.currentTaskIndex == 2) {
            UpArrow.SetActive(true);
        } else if (syringe.IsNozzleDipInsidePEGTube() && waterAmount > 60 && _flowController.currentTaskIndex == 7) {
            DownArrow.SetActive(true);
        } else {
            UpArrow.SetActive(false);
            DownArrow.SetActive(false);
        }

    }

    bool isValiPosition() {
        return syringe.IsNozzleDipValid();// || condition for placed on tube
    }

    private float CalculateWater() {

        float currentPos = syringeHandleGrab.transform.localPosition.z;
        return ((currentPos - p1) / h) * 100;
    }

    public void ResetSyringe() {
        Vector3 _resetpos = syringeGrabTransformer.transform.localPosition;
        _resetpos.z = syringeGrabTransformer.Constraints.MaxZ.Value;
        syringeGrabTransformer.transform.localPosition = _resetpos;
        syringe.FillWater(0);
    }

}
