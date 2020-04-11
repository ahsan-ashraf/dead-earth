using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIState : MonoBehaviour {

    protected AIStateMachine StateMachine = null;

    public abstract AIStateType GetStateType();
    public abstract AIStateType OnUpdate();

    public virtual void OnEnterState()      { }
    public virtual void OnExitState()       { }
    public virtual void OnAnimatorUpdated() { }
    public virtual void OnTriggerEvent(AITriggerEvent eventType, Collider other) { }
    public virtual void OnDestinationReached(bool isReached) { }

    public void SetStateMachine(AIStateMachine stateMachine) {
        StateMachine = stateMachine;
    }
}
