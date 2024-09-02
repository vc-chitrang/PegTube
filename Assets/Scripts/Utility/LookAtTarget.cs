using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTarget : MonoBehaviour {
    private enum RotationAxis {
        X, Y, Z, ALL
    }
    private Transform myTransform;
    [SerializeField] private bool lookAtCamera;
    [SerializeField] private Transform target;
    [SerializeField] private RotationAxis rotationAxis;
    private Vector3 targetPosition;
    [SerializeField] private Vector3 paddingRotation;

    // Start is called before the first frame update
    private void Start() {
        myTransform = transform;
        if (lookAtCamera)
            target = Camera.main.transform;
    }

    // Update is called once per frame
    private void LateUpdate() {
        if (target != null) {
            RotateFaceToTarget();
        }
    }

    public void SetTarget(Transform chosenTarget) {
        target = chosenTarget;
    }

    private void RotateFaceToTarget() {
        switch (rotationAxis) {
            case RotationAxis.X:
                targetPosition = new Vector3(myTransform.position.x, target.position.y, target.position.z);
                myTransform.LookAt(targetPosition, Vector3.right);
                break;
            case RotationAxis.Y:
                targetPosition = new Vector3(target.position.x, myTransform.position.y, target.position.z);
                myTransform.LookAt(targetPosition, Vector3.up);

                break;
            case RotationAxis.Z:
                targetPosition = new Vector3(target.position.x, target.position.y, myTransform.position.z);
                myTransform.LookAt(targetPosition, Vector3.forward);
                break;
            case RotationAxis.ALL:
                myTransform.LookAt(target);
                break;
        }
        myTransform.eulerAngles -= paddingRotation;
    }

    
}
