using UnityEngine;

public class SyringeNozzle : MonoBehaviour {
    private bool dippedIntoWater = false;
    private const string waterTag = "water";
    [SerializeField]
    private string waterFillObjectName = "water";
    [SerializeField]
    private string waterInjectObjectName = "water";
    private string currentDippedObjectName = "water";
    [SerializeField]
    private MeshRenderer MeshRenderer;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag(waterTag)) {
            dippedIntoWater = true;
            currentDippedObjectName = other.name;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag(waterTag)) {
            dippedIntoWater = false;
            currentDippedObjectName = "";
        }
    }

    internal bool IsDeepedInotWater() {
        return dippedIntoWater;
    }
    internal bool IsFillEnabled() {
        return currentDippedObjectName == waterFillObjectName;
    }
    internal bool IsInjectEnabled() {
        return currentDippedObjectName == waterInjectObjectName;
    }
}
