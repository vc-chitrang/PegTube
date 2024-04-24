using UnityEngine;

public class SyringeNozzle : MonoBehaviour {
    private bool dippedIntoWater = false;
    private const string waterTag = "water";
    [SerializeField]
    private MeshRenderer MeshRenderer;
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag(waterTag)) {
            dippedIntoWater = true;
            MeshRenderer.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag(waterTag)) {
            dippedIntoWater = false;
            MeshRenderer.enabled = false;
        }
    }

    internal bool IsDeepedInotWater() {
        return dippedIntoWater;
    }
}
