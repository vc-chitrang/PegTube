using System;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour {

    [SerializeField]
    private int executeStepIndex = 0;

    [SerializeField]
    private List<Step> steps = new List<Step>();

    [SerializeField]
    private Queue<Step> queue = new Queue<Step>();

    private void Start() {
        queue.Clear();

        int stepIndex = 0;
        steps.ForEach(step => {
            step.stepIndex = stepIndex;
            step.information = step.State.ToString();

            queue.Enqueue(step);
            stepIndex++;
        });
    }

    public void ExecuteStep(int stepIndex) {
        Step peekedStep = queue.Peek();

        if (peekedStep.stepIndex == stepIndex) {
            Step currentStep = queue.Dequeue();
            currentStep.Execute();
            currentStep.PrintExecutedMessage();
        } else {
            Debug.LogError($"ERROR: Wrong Step Selected!!! \n Please {peekedStep.State}");
        }
    }

    [ContextMenu("Execute")]
    public void Execute() {
        ExecuteStep(executeStepIndex);
    }

}//StateManager class end.

[Serializable]
public class Step {
    public string information;
    public int stepIndex;
    public State State;
    public bool isCompleted;

    public void Reset() {
        isCompleted = false;
    }

    internal void Execute() {
        isCompleted = true;
    }

    public void PrintExecutedMessage() {
        Debug.Log($"Executed {stepIndex}: {information}");
    }
}

public enum State {
    None = 0,
    FillSyringeWithWater = 1,
    CloseTheClamp = 2,
    OpenThePort = 3,
    AttachTheSyringe = 4,
    OpenTheClamp = 5,
    PushTheSyringePlunger = 6,
    CloseTheClampFinal = 7,
    RemoveTheSyringe = 8,
    CloseThePort = 9,
    Done = 10
}
