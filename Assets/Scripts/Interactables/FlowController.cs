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

    public float welcomePeddingTime = 2f;
    public float instroductionOfEquipemntTime = 2f;

    public List<Task> tasks = new List<Task>();
    public List<GameObject> allEquipmentIntroductions = new List<GameObject>();
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
    bool WelcomePlayed = false;
    Coroutine currentTaskCoroutine;

    public AudioSource instructionAudioSource;
    public AudioClip trainingCompleteAudioClip;
    public AudioClip invalidMoveAudioClip;

    private const string TrainingCompleteMessage = "<b>Training Complete!</b>\nWould you like to try again?\r\n<size=.4>Remember, the PEG tube should ideally be flushed before and after administering medications, after feeding, and routinely every 4-6 hours if the tube is not in continuous use.</size>";

    private const string InvalidMoveMessage = "<b>Invalid Action!</b>\r\nThis could jeopardize the patient's safety.\r\nPlease restart the training.";

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

        if (tasks[currentTaskIndex].highlightObjectKeys.Count > 0) {
            foreach (string highlight in tasks[currentTaskIndex].highlightObjectKeys) {
                AddHighlight(highlight);
            }
        }
        if (tasks[currentTaskIndex].demoVideoClip != null)
            PlayVideo(tasks[currentTaskIndex].demoVideoClip);
        else {
            StopVideo();
        }
        if (tasks[currentTaskIndex].instructionAudioClip != null) {
            PlayVoiceOverInstruction(tasks[currentTaskIndex].instructionAudioClip);
        } else {
            StopVoiceOverInstruction();
        }
        float paddingTime = 2;

        switch (currentTaskIndex) {
            case 0://welcome
                if (!WelcomePlayed) {
                    yield return new WaitWhile(IsAudioPlaying);// wait whlie audio playing
                    yield return new WaitForSeconds(welcomePeddingTime);
                    yield return new WaitForSeconds(paddingTime);
                }
                break;
            case 1://Introduction To Equipments
                if (!WelcomePlayed) {
                    // enable Introductions 
                    ToggleAllEquipmentIntrodutions(true);
                    yield return new WaitWhile(IsAudioPlaying);// wait whlie audio playing
                    yield return new WaitForSeconds(instroductionOfEquipemntTime);
                    ToggleAllEquipmentIntrodutions(false);
                    yield return new WaitForSeconds(paddingTime);

                    WelcomePlayed = true;
                }
                break;
            case 2://Pick up Big Injection and Fill Water
                if (IsInjectionEmpty()) {
                    yield return new WaitWhile(IsInjectionEmpty);
                    yield return new WaitForSeconds(paddingTime);
                }
                break;

            case 3://Close Clamp
                StartCoroutine(HandleFailCasesForInjection());
                if (!isClampLocked()) {
                    yield return StartCoroutine(WaitForClampToggle(false));
                    yield return new WaitForSeconds(paddingTime);
                }
                break;

            case 4://Open Valve
                if (!IsValveOpen()) {
                    yield return new WaitUntil(IsValveOpen);
                    yield return new WaitForSeconds(paddingTime);
                }
                break;

            case 5://Plug Filled Injection on Valve
                if (!IsInjectionSnapped()) {
                    yield return new WaitUntil(IsInjectionSnapped);
                    yield return new WaitForSeconds(paddingTime);
                }
                break;

            case 6://Open Clamp 
                if (isClampLocked()) {
                    yield return StartCoroutine(WaitForClampToggle(true));
                    yield return new WaitForSeconds(paddingTime);
                }
                break;

            case 7://Inject Water
                if (!IsInjectionEmpty()) {
                    yield return new WaitUntil(IsInjectionEmpty);
                    yield return new WaitForSeconds(paddingTime);
                }
                waterInjected = true;
                break;

            case 8://close Clamp again 
                if (!isClampLocked()) {
                    yield return StartCoroutine(WaitForClampToggle(false));
                    yield return new WaitForSeconds(paddingTime);
                }
                break;

            case 9://remove Injection
                if (IsInjectionSnapped()) {
                    yield return new WaitWhile(IsInjectionSnapped);
                    yield return new WaitForSeconds(paddingTime);
                }
                break;

            case 10://Close Valve
                if (IsValveOpen()) {
                    yield return new WaitWhile(IsValveOpen);
                    yield return new WaitForSeconds(paddingTime);
                }
                break;

            default:
                Debug.Log("No more tasks");
                break;
        }
        //yield return new WaitForSeconds(2f);
        setTaskIndex(currentTaskIndex + 1);
        yield return new WaitForEndOfFrame();

    }
    private void PlayVideo(VideoClip clip) {
        StartCoroutine(PlayDemoVideo(clip));
    }
    private void ToggleAllEquipmentIntrodutions(bool enable) {
        foreach (GameObject obj in allEquipmentIntroductions) {
            obj.SetActive(enable);
        }
    }
    void StopVideo() {
        if (videoPlayer.isPlaying || videoPlayer.isPaused) {
            videoPlayer.Stop();
        }
    }
    IEnumerator PlayDemoVideo(VideoClip clip) {
        StopVideo();
        yield return new WaitForEndOfFrame();
        videoPlayer.clip = clip;
        videoPlayer.Prepare();
        yield return new WaitUntil(() => videoPlayer.isPrepared);
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
            instructionText.text = ((currentTaskIndex - 1 > 0) ? (currentTaskIndex - 1) + ". " : "") + tasks[currentTaskIndex].instruction;
            currentTaskCoroutine = StartCoroutine(CheckCurrentTask());
        } else {
            //RemoveAllHighlights();
            //StopAllCoroutines();
            PlayVoiceOverInstruction(trainingCompleteAudioClip);
            ShowRestart(TrainingCompleteMessage);
            //instructionText.text = "Training Completed";
            //videoPlayer.Stop();
            Debug.Log("Complete");
        }
    }

    private IEnumerator HandleFailCasesForInjection() {
        int taskIndex = 2;
        int endIndex = 7;

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

    private void ShowRestart(string message = InvalidMoveMessage) {
        if (message == InvalidMoveMessage) {
            PlayVoiceOverInstruction(invalidMoveAudioClip);
        }
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

    private void StopVoiceOverInstruction() {
        if (instructionAudioSource.isPlaying) {
            instructionAudioSource.Stop();
        }
    }
    private void PlayVoiceOverInstruction(AudioClip audio) {
        StopVoiceOverInstruction();
        instructionAudioSource.clip = audio;
        instructionAudioSource.Play();
    }

    private bool IsAudioPlaying() {
        return instructionAudioSource.isPlaying;
    }
}

[Serializable]
public class Task {
    private static int lastTaskId = 0;

    public int taskId;
    public string instruction;
    public List<string> highlightObjectKeys;
    public VideoClip demoVideoClip;
    public AudioClip instructionAudioClip;

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
