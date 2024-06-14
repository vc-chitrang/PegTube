using UnityEngine;

public class Syringe : InteractableObjectBase {
    [SerializeField]
    private SkinnedMeshRenderer skinnedMeshRenderer;

    [SerializeField]
    private SyringeNozzle _syringeNozzle;

    private float _fillAmount = 0;

    [Range(0, 100)]
    public float waterFillAmount = 0;

    private void Start() {
        FillWater(0);    
    }

    internal void FillWater(float waterAmount) {
        //if (waterAmount < _fillAmount) {
        //    return;
        //}
        skinnedMeshRenderer.SetBlendShapeWeight(0, waterAmount);
        waterFillAmount = waterAmount;
    }

    internal bool IsNozzleDipInsideWaterCup() {
        return _syringeNozzle.IsDeepedInotWater();
    }
    
}//Syringe class end.
