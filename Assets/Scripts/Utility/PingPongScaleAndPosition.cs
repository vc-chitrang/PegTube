using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingPongScaleAndPosition : MonoBehaviour
{
    public float minScale = 1f;
    public float maxScale = 2f;
    public float scaleSpeed = 1f;
    public float positionOffset = 0.5f;
    public float positionAmplitude = 0.5f;
    public float positionSpeed = 1f;
    public float rotationSpeed = 45f; // Degrees per second

    public Transform objectTansform;
    
    private float scaleTimer = 0f;
    private float positionTimer = 0f;

    private void Start() {
        
    }

    private void Update() {
        // Ping-pong scaling
        scaleTimer += Time.deltaTime * scaleSpeed;
        float currentScale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(scaleTimer) + 1f) / 2f);
        objectTansform.localScale = new Vector3(currentScale, currentScale, currentScale);

        // Ping-pong position
        positionTimer += Time.deltaTime * positionSpeed;
        float yPosition = positionAmplitude * Mathf.Sin(positionTimer);
        objectTansform.localPosition = new Vector3(objectTansform.localPosition.x, positionOffset+ yPosition, objectTansform.localPosition.z);

        // Continuous rotation
        float rotation = rotationSpeed * Time.time;
        objectTansform.localRotation = Quaternion.Euler(90f, 0f, rotation);
    }
}
