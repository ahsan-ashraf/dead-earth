using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
/// <summary>
/// Generic Patrolling Behaviour for a Zombie.
/// </summary>
public class AIZombieState_Patrol1 : AIZombieState {

    // Inspector Assigned
    [SerializeField] private float              TurnOnSpotThreshold     = 80.0f;
    [SerializeField] private float              SlerpSpeed              = 5.0f;

    [SerializeField] [Range(0.0f, 3.0f)] private float Speed = 1;
    
    /// <summary>
    /// Returns this state's type.
    /// Called by State Machine.
    /// </summary>
    /// <returns></returns>
    public override AIStateType GetStateType() {
        return (AIStateType.Patrol);
    }
    /// <summary>
    /// Called once by State Machine when transitioning into this state, to initialize this state.
    /// </summary>
    public override void OnEnterState() {
        base.OnEnterState();
        Debug.Log("Entering Patrol State");
        if (ZombieStateMachine == null) return;

        ZombieStateMachine.NavAgentControl(true, false);
        ZombieStateMachine.speed        = Speed;
        ZombieStateMachine.seeking      = 0;
        ZombieStateMachine.feeding      = false;
        ZombieStateMachine.attackType   = 0;

        // Set Destination
        ZombieStateMachine.navAgent.SetDestination(ZombieStateMachine.GetWayPointPosition(false));

        ZombieStateMachine.navAgent.isStopped = false;
    }
    /// <summary>
    /// Called by State Machine each frame to give this state a time-slice to update itself.
    /// Returns type of this state.
    /// </summary>
    /// <returns></returns>
    public override AIStateType OnUpdate() {
        if (ZombieStateMachine == null) {
            return (AIStateType.Idle);
        } else {
            // Do we have a visual threat that is player.
            if (ZombieStateMachine.VisualThreat.type == AITargetType.Visual_Player) {
                ZombieStateMachine.SetTarget(ZombieStateMachine.VisualThreat);
                return (AIStateType.Pursuit);
            }
            if (ZombieStateMachine.VisualThreat.type == AITargetType.Visual_Light) {
                ZombieStateMachine.SetTarget(ZombieStateMachine.VisualThreat);
                return (AIStateType.Alerted);
            }
            // Sound is third highest priority threar.
            if (ZombieStateMachine.AudioThreat.type == AITargetType.Audio) {
                ZombieStateMachine.SetTarget(ZombieStateMachine.AudioThreat);
                return (AIStateType.Alerted);
            }
            // If zombie is hungry and saw a dead body then pursue towards it.
            if (ZombieStateMachine.VisualThreat.type == AITargetType.Visual_Food) {
                
                if (1.0f - ZombieStateMachine.satisfaction > ZombieStateMachine.VisualThreat.distance / ZombieStateMachine.sensorRadius) {
                    ZombieStateMachine.SetTarget(ZombieStateMachine.VisualThreat);
                    Debug.Log("Food found returning pursuit state");
                    return (AIStateType.Pursuit);
                }
            }
            float angle = Vector3.Angle(ZombieStateMachine.transform.forward, (ZombieStateMachine.navAgent.steeringTarget - ZombieStateMachine.transform.position));
            if (angle > TurnOnSpotThreshold) {
                return (AIStateType.Alerted);
            }
            if (!ZombieStateMachine.useRootRotation) {
                Quaternion newRotation = Quaternion.LookRotation(ZombieStateMachine.navAgent.desiredVelocity, Vector3.up);
                ZombieStateMachine.transform.rotation = Quaternion.Slerp(ZombieStateMachine.transform.rotation, newRotation, SlerpSpeed * Time.deltaTime);
            }
            if (ZombieStateMachine.navAgent.isPathStale || !ZombieStateMachine.navAgent.hasPath || ZombieStateMachine.navAgent.pathStatus != NavMeshPathStatus.PathComplete) {
                ZombieStateMachine.GetWayPointPosition(true);
            }
            return (AIStateType.Patrol);
        }
    }
    /// <summary>
    /// Called by the parent StateMachine when the zombie has reached
	/// its target entered its target trigger.
    /// </summary>
    public override void OnDestinationReached(bool isReached) {
        if (ZombieStateMachine == null || !isReached) {
            return;
        }
        if (ZombieStateMachine.targetType == AITargetType.WayPoint) {
            ZombieStateMachine.GetWayPointPosition(true);
        }
    }
    /// <summary>
    /// OnAnimatorIKUpdated
	/// Override IK Goals
    /// </summary>
    /*
    public override void OnAnimatorIKUpdated() {
        if (ZombieStateMachine == null) return;

        ZombieStateMachine.animator.SetLookAtPosition(ZombieStateMachine.targetPosition + Vector3.up);
        ZombieStateMachine.animator.SetLookAtWeight(0.55f);
    }
    */
}
