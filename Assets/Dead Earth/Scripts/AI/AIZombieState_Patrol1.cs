using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
/// <summary>
/// Generic Patrolling Behaviour for a Zombie.
/// </summary>
public class AIZombieState_Patrol1 : AIZombieState {

    // Inspector Assigned
    [SerializeField] private AIWayPointNetwork  WayPointNetWrok         = null;
    [SerializeField] private bool               RandomPatrol            = false;
    [SerializeField] private int                CurrentWayPointIndex    = 0;
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

        // If current target isn't a waypoint then we need to select a waypoint 
        // from the waypoint network and make this the new target and plot a path to it.
        if (ZombieStateMachine.targetType != AITargetType.WayPoint) {
            ZombieStateMachine.ClearTarget();   // Clear any previous target.

            // Do we have a valid waypoint network.
            if (WayPointNetWrok != null && WayPointNetWrok.WayPoints.Count > 0) {

                // If its a random patrol then select a random index.
                UpdateWayPoint();
            }
        }
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
                UpdateWayPoint();
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
            UpdateWayPoint();
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
    /// <summary>
    /// Called to select a new waypoint. Either randomly selects a new waypoint from the waypoint network or increments the current
    /// waypoint index (with wrap-around) to visit the waypoints in the network in sequence. Sets the new waypoint as the the
    /// target and generates a nav agent path for it.
    /// </summary>
    private void UpdateWayPoint() {
        // If its a random patrol then select a random index.
        if (RandomPatrol && WayPointNetWrok.WayPoints.Count > 1) {
            int oldWayPoint = CurrentWayPointIndex;
            while (CurrentWayPointIndex == oldWayPoint) {
                CurrentWayPointIndex = Random.Range(0, WayPointNetWrok.WayPoints.Count);
            }
        } else {
            CurrentWayPointIndex = CurrentWayPointIndex == WayPointNetWrok.WayPoints.Count - 1 ? 0 : CurrentWayPointIndex + 1;
        }
        Transform newWaypoint = WayPointNetWrok.WayPoints[CurrentWayPointIndex];
        if (newWaypoint != null) {
            ZombieStateMachine.SetTarget(AITargetType.WayPoint, null, newWaypoint.position, Vector3.Distance(ZombieStateMachine.transform.position, newWaypoint.position));
            ZombieStateMachine.navAgent.SetDestination(newWaypoint.position);
        }
    }
}
