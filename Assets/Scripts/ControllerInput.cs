using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerInput : MonoBehaviour {
    public InputActionProperty rightThumbstick;  // Public property    
    public Syringe Syringe;
    
    private void Update() {
        if (!Syringe.IsNozzleDipInsideWaterCup()) {
            return;
        }

        Vector2 thumbstickValue = rightThumbstick.action.ReadValue<Vector2>();

        float _mappedValue = MathUtility.MapValue(Mathf.Abs(thumbstickValue.y), 0, 1, 0, 100f);
        Syringe.FillWater(_mappedValue);
    }
}
