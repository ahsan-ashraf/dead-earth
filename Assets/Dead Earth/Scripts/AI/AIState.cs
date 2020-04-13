using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Base class of all AI States used by our AI System.
/// </summary>
public abstract class AIState : MonoBehaviour {

    protected AIStateMachine StateMachine = null;

    /// <summary>
    /// Returns the type of currently active state i.e. StateType.
    /// </summary>
    /// <returns></returns>
    public abstract AIStateType GetStateType();
    /// <summary>
    /// Called once per frame of currently active state.
    /// </summary>
    /// <returns></returns>
    public abstract AIStateType OnUpdate();
    /// <summary>
    /// Called once in the first frame of currently active state.
    /// </summary>
    public virtual void OnEnterState()          { }
    /// <summary>
    /// Called once in the last frame (i.e. when this state is trasits 
    /// into another state) of currently active state.
    /// </summary>
    public virtual void OnExitState()           { }
    public virtual void OnTriggerEvent(AITriggerEventType eventType, Collider other) { }
    public virtual void OnDestinationReached(bool isReached) { }
    /// <summary>
    /// Called by AIStateMachine base class in OnAnimatorIK() function.
    /// </summary>
    public virtual void OnAnimatorIKUpdated() { }
    /// <summary>
    /// Called by AIStateMachine base class in OnAnimatorMove() function.
    /// Provides default functionality for updating root position and root 
    /// rotation based on root motion information stored in animation clip.
    /// </summary>
    public virtual void OnAnimatorUpdated() {
        if (StateMachine.useRootPosition) {
            StateMachine.navAgent.velocity = StateMachine.animator.deltaPosition / Time.deltaTime;
        }
        if (StateMachine.useRootRotation) {
            StateMachine.gameObject.transform.rotation = StateMachine.animator.rootRotation;
        }
    }
    /// <summary>
    /// Sets StateMachine reference variable to the provided AIStateMachine.
    /// </summary>
    /// <param name="stateMachine"></param>
    public void SetStateMachine(AIStateMachine stateMachine) {
        StateMachine = stateMachine;
    }
}
