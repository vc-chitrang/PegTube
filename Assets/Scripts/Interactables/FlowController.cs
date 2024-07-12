using Oculus.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;

public class FlowController : MonoBehaviour {
    public int currentTaskIndex = 0;
    public List<Task> tasks = new List<Task>();
    public VideoClip restartVideoClip;

    [Header("UI")]
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Objects")]
    [SerializeField] private SyringeHandController _syringeHandController;
    [SerializeField] private ClampHandController _clampHandController;
    [SerializeField] private ValveHandController _valveHandController_primary;
    [SerializeField] private SnapInteractable _snapInteractable_primary;
    [SerializeField] private ValveHandController _valveHandController_secondary;
    [SerializeField] private SnapInteractable _snapInteractable_secondary;
    public SerializedDictionary<string, QuickOutline> HighlightObjects;
    bool waterInjected = false;
    Coroutine currentTaskCoroutine;

    // Start is called before the first frame update
    void Start() {
        waterInjected = false;
        setTaskIndex(0);
        emptyInjectionSnapCoroutine = StartCoroutine(CheckIfSnappedInjectionRemoved());
    }

    // Update is called once per frame
    void Update() {
    }

    public IEnumerator CheckCurrentTask() {
        RemoveAllHighlights();
        foreach (string highlight in tasks[currentTaskIndex].highlightObjectKeys) {
            AddHighlight(highlight);
        }
        PlayVideo(tasks[currentTaskIndex].demoVideoClip);

        switch (currentTaskIndex) {
            case 0://Pick up Big Injection and Fill Water
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
                waterInjected = true;
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
        setTaskIndex(currentTaskIndex + 1);
        yield return new WaitForEndOfFrame();

    }
    private void PlayVideo(VideoClip clip) {
        StartCoroutine(PlayDemoVideo(clip));
    }

    IEnumerator PlayDemoVideo(VideoClip clip) {
        if (videoPlayer.isPlaying || videoPlayer.isPaused) {
            videoPlayer.Stop();
        }
        yield return new WaitForEndOfFrame();
        videoPlayer.clip = clip;
        videoPlayer.Prepare();
        yield return new WaitUntil(()=>videoPlayer.isPrepared);
        videoPlayer.Play();
    }

    public void RemoveAllHighlights() {
        for (int i = 0; i < HighlightObjects.Count; i++) {
            HighlightObjects.ElementAt(i).Value.enabled = false;
        }
    }
    public void AddHighlight(string key) {
        if (HighlightObjects.TryGetValue(key, out QuickOutline outline))
            if (outline != null) {
                outline.enabled = true;
            }
    }

    public void RemoveHighlight(string key) {
        if (HighlightObjects.TryGetValue(key, out QuickOutline outline))
            if (outline != null) {
                outline.enabled = false;
            }
    }

    private bool IsInjectionSnapped() {
        return _snapInteractable_primary.State == InteractableState.Select || _snapInteractable_secondary.State == InteractableState.Select;
    }

    private bool IsInjectionEmpty() {
        return _syringeHandController.syringe.waterFillAmount <= 10f;
    }

    private bool IsValveOpen() {
        return _valveHandController_primary.valveIsOpen || _valveHandController_secondary.valveIsOpen;
    }

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
            //RemoveAllHighlights();
            //StopAllCoroutines();
            ShowRestart("Training Completed! Would you like to try again ?");
            //instructionText.text = "Training Completed";
            //videoPlayer.Stop();
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
            ShowRestart();
        }
    }
    public void OnValveOpen() {
        if (!_clampHandController._isLocked) {
            Debug.Log("Critical Mistake Restart Training ");
            ShowRestart();
        }
    }

    private void ShowRestart(string message = "Invalid Move! This can be critical to the patient!!\nRestart Training.") {
        StopAllCoroutines();
        RemoveAllHighlights();
        instructionText.text = message;
        restartButton.SetActive(true);
        PlayVideo(restartVideoClip);
    }

    public void RestartTraining() {
        restartButton.SetActive(false);
        _syringeHandController.ResetSyringe();
        _clampHandController.LockClump(false);
        _valveHandController_primary.ResetValve();
        _valveHandController_secondary.ResetValve();
        Start();
    }
    Coroutine emptyInjectionSnapCoroutine;
    IEnumerator CheckIfSnappedInjectionRemoved() {
        yield return new WaitUntil(IsInjectionSnapped);
        if (IsInjectionEmpty() && !waterInjected) {
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
            emptyInjectionSnapCoroutine = StartCoroutine(CheckIfSnappedInjectionRemoved());
        }
    }

}

[Serializable]
public class Task {
    private static int lastTaskId = 0;

    public int taskId;
    public string instruction;
    public List<string> highlightObjectKeys;
    public VideoClip demoVideoClip;

    public Task() {
        taskId = ++lastTaskId;
    }
}

[Serializable]
public class PointerData {
    public List<Pointer> pointers;
}

[Serializable]
public class Pointer {
    public int TaskIndex;
    public GameObject pointerObject;
}
