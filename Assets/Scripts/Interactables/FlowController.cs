using Oculus.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FlowController : MonoBehaviour {
    public int currentTaskIndex = 0;
    public List<Task> tasks = new List<Task>();

    [Header("UI")]
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private GameObject restartButton;
    [Header("Objects")]

    [SerializeField] private SyringeHandController _syringeHandController;
    [SerializeField] private ClampHandController _clampHandController;
    [SerializeField] private ValveHandController _valveHandController_primary;
    [SerializeField] private SnapInteractable _snapInteractable_primary;
    [SerializeField] private ValveHandController _valveHandController_secondary;
    [SerializeField] private SnapInteractable _snapInteractable_secondary;
    Coroutine currentTaskCoroutine;

    // Start is called before the first frame update
    void Start() {
        setTaskIndex(0);
        StartCoroutine(CheckIfSnappedInjectionRemoved());
    }

    // Update is called once per frame
    void Update() {
    }

    public IEnumerator CheckCurrentTask() {

        switch (currentTaskIndex) {
            case 0://Pick up Big Injection and Fill Water
                //yield return StartCoroutine(WaitForInjectionFill());
                yield return new WaitWhile(IsInjectionEmpty);
                break;
            case 1://Close Clamp
                StartCoroutine(HandleFailCasesForInjection());
                yield return StartCoroutine(WaitForClampToggle(false));

                break;

            case 2://Open Valve
                yield return new WaitUntil(IsValveOpen);

                break;

            case 3://Plug Filled Injection on Valve
                yield return new WaitUntil(IsInjectionSnapped);
                break;

            case 4://Open Clamp 
                yield return StartCoroutine(WaitForClampToggle(true));
                break;

            case 5://Inject Water
                yield return new WaitUntil(IsInjectionEmpty);
                break;

            case 6://close Clamp again 
                yield return StartCoroutine(WaitForClampToggle(false));
                break;

            case 7://remove Injection
                yield return new WaitWhile(IsInjectionSnapped);
                break;

            case 8://Close Valve
                yield return new WaitWhile(IsValveOpen);
                break;

            default:
                Debug.Log("No more tasks");
                break;
        }
        //yield return new WaitForSeconds(1);
        setTaskIndex(currentTaskIndex + 1);

    }

    private void CheckFailTaskFlow() {

    }
    //[ContextMenu("test")]
    //public void test() {
    //    Debug.Log("IsInjectionSnapped" + IsInjectionSnapped());
    //}
    private bool IsInjectionSnapped() {
        //return _syringeHandController.syringe.isSnaped;
        return _snapInteractable_primary.State == InteractableState.Select || _snapInteractable_secondary.State == InteractableState.Select; //_snapInteractable_primary.Interactors.Count >  0 || _snapInteractable_secondary.Interactors.Count > 0;
    }

    private bool IsInjectionEmpty() {
        return _syringeHandController.syringe.waterFillAmount <= 10f;
    }

    private bool IsValveOpen() {
        return _valveHandController_primary.valveIsOpen || _valveHandController_secondary.valveIsOpen;
    }

    //private bool IsAllValveClosed() {
    //    return (!_valveHandController_primary.valveIsOpen) && (!_valveHandController_secondary.valveIsOpen);
    //}

    private IEnumerator WaitForClampToggle(bool _isOpen) {
        if (_isOpen) {
            yield return new WaitWhile(isClampLocked);
        } else {
            yield return new WaitUntil(isClampLocked);
        }
    }

    private bool isClampLocked() {
        return _clampHandController._isLocked;
    }

    private void setTaskIndex(int index) {
        if (index < tasks.Count) {
            currentTaskIndex = index;
            Debug.Log("Current Task changed to : " + currentTaskIndex);
            // set task instruction and other data here
            instructionText.text = (currentTaskIndex + 1) + ". " + tasks[currentTaskIndex].instruction;
            currentTaskCoroutine = StartCoroutine(CheckCurrentTask());
        } else {
            instructionText.text = "Training Completed";
            Debug.Log("Complete");
        }
    }

    private IEnumerator HandleFailCasesForInjection() {
        int taskIndex = 0;
        int endIndex = 5;

        if (currentTaskIndex > taskIndex && currentTaskIndex <= endIndex) {
            yield return new WaitUntil(IsInjectionEmpty);

            if (currentTaskIndex != endIndex) {
                Debug.Log("Injection Empty bot allowed Fill Again ");
                currentTaskIndex = taskIndex;
                StopCoroutine(currentTaskCoroutine);
                setTaskIndex(taskIndex);
            }

        }
    }

    public void OnClampOpen() {
        if ((_valveHandController_primary.valveIsOpen && _valveHandController_secondary.valveIsOpen) || (_valveHandController_primary.valveIsOpen && _snapInteractable_primary.State != InteractableState.Select) || (_valveHandController_secondary.valveIsOpen && _snapInteractable_secondary.State != InteractableState.Select)) {
            //Debug.Log("Critical Mistake Restart Training ");
            ShowRestart();
        }
    }
    public void OnValveOpen() {
        if (!_clampHandController._isLocked) {
            Debug.Log("Critical Mistake Restart Training ");
            ShowRestart();
        }
    }
    public void test() {
        //_snapInteractable_primary.O.

    }

    private void ShowRestart(string message = "Invalid Move! This can be critical to the patient!!\nRestart Training.") {
        StopAllCoroutines();
        instructionText.text = message;
        restartButton.SetActive(true);
    }

    public void RestartTraining() {
        restartButton.SetActive(false);
        _syringeHandController.ResetSyringe();
        _clampHandController.LockClump(false);
        _valveHandController_primary.ResetValve();
        _valveHandController_secondary.ResetValve();
        Start();
    }

    IEnumerator CheckIfSnappedInjectionRemoved() {
        yield return new WaitUntil(IsInjectionSnapped);
        if (IsInjectionEmpty()) {
            ShowRestart("Injection is Empty. Please Fill then Try Again.\nRestart Training.");
        }
        yield return new WaitWhile(IsInjectionSnapped);
        OnDetachSyringe();
    }
    public void OnDetachSyringe() {
        if (!_clampHandController._isLocked) {
            Debug.Log("Critical Mistake Restart Training ");
            ShowRestart();
        } else {

            StartCoroutine(CheckIfSnappedInjectionRemoved());
        }
    }

}

[Serializable]
public class Task {

    public int TaskId;
    public string instruction;
    public int failIndex;
}