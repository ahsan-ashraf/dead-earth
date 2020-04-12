using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIState : MonoBehaviour {

    protected AIStateMachine StateMachine = null;

    public abstract AIStateType GetStateType();
    public abstract AIStateType OnUpdate();

    public virtual void OnEnterState()          { }
    public virtual void OnExitState()           { }
    public virtual void OnAnimatorUpdated()     { }
    public virtual void OnAnimatorIKUpdated()   { }
    public virtual void OnTriggerEvent(AITriggerEventType eventType, Collider other) { }
    public virtual void OnDestinationReached(bool isReached) { }

    /// <summary>
    /// Sets StateMachine reference variable to the provided AIStateMachine.
    /// </summary>
    /// <param name="stateMachine"></param>
    public void SetStateMachine(AIStateMachine stateMachine) {
        StateMachine = stateMachine;
    }
}
