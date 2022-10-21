using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    private State currentState;

    void Start()
    {
        currentState = GetInitialState();
        if (currentState != null) currentState.Enter();
    }

    void Update()
    {
        if (currentState != null) currentState.UpdateLogic();
    }

    void LateUpdate()
    {
        if (currentState != null) currentState.UpdatePhysics();
    }

    // exits the current state and enters new one
    public void ChangeState(State newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }

    protected virtual State GetInitialState()
    {
        return null;
    }

    // displays what state guard is in -> FOR DEBUGGING ONLY
    // private void OnGUI()
    // {
    //     string content = currentState != null ? currentState.name : "(no current state)";
    //     GUILayout.Label($"<color='black'><size=40>{content}</size></color>");
    // }
}