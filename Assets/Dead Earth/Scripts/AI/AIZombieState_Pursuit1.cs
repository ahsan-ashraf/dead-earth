using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIZombieState_Pursuit1 : AIZombieState {

    [SerializeField] [Range(0.0f, 10.0f)] private float Speed = 1.0f;

    [SerializeField] private float SlerpSpeed               = 5.0f;
    [SerializeField] private float RepathDistanceMultiplier = 0.035f;
    [SerializeField] private float RepathVisualMinDuration  = 0.05f;
    [SerializeField] private float RepathVisualMaxDuration  = 5.0f;
    [SerializeField] private float RepathAudioMinDuration   = 0.25f;
    [SerializeField] private float RepathAudioMaxDuration   = 5.0f;
    [SerializeField] private float MaxDuration              = 40.0f;

    private float   Timer           = 0.0f;
    private float   RepathTimer     = 0.0f;

    public override AIStateType GetStateType() {
        return (AIStateType.Pursuit);
    }
    public override void OnEnterState() {
        base.OnEnterState();
        Debug.Log("Entering Pursuit State");
        if (ZombieStateMachine == null)
            return;

        // Configure Pursuit State.
        ZombieStateMachine.NavAgentControl(true, false);
        ZombieStateMachine.speed        = Speed;
        ZombieStateMachine.seeking      = 0;
        ZombieStateMachine.feeding      = false;
        ZombieStateMachine.attackType   = 0;
        Timer                           = 0.0f;
        RepathTimer                     = 0.0f;

        // Set Path.
        ZombieStateMachine.navAgent.SetDestination(ZombieStateMachine.targetPosition);
        ZombieStateMachine.navAgent.isStopped = false;
    }
    public override AIStateType OnUpdate() {
        Timer += Time.deltaTime;
        RepathTimer += Time.deltaTime;

        if (Timer > MaxDuration) {
            return (AIStateType.Patrol);
        }

        if (ZombieStateMachine.targetType == AITargetType.Visual_Player && ZombieStateMachine.inMeleeRange) {
            return (AIStateType.Attack);
        }

        if (ZombieStateMachine.targetReached) {
            switch (ZombieStateMachine.targetType) {
                case AITargetType.Audio:
                case AITargetType.Visual_Light:
                    ZombieStateMachine.ClearTarget();
                    return (AIStateType.Alerted);
                case AITargetType.Visual_Food:
                    return (AIStateType.Feeding);
            }
        }

        if (ZombieStateMachine.navAgent.isPathStale || !ZombieStateMachine.navAgent.hasPath || ZombieStateMachine.navAgent.pathStatus != NavMeshPathStatus.PathComplete) {
            return (AIStateType.Alerted);
        }

        if (!ZombieStateMachine.useRootRotation && ZombieStateMachine.targetType == AITargetType.Visual_Player && 
            ZombieStateMachine.VisualThreat.type == AITargetType.Visual_Player && ZombieStateMachine.targetReached) {
            Vector3 targetPosition  = ZombieStateMachine.targetPosition;
            targetPosition.y        = ZombieStateMachine.transform.position.y;
            Quaternion newRotation  = Quaternion.LookRotation((targetPosition - ZombieStateMachine.transform.position), Vector3.up);
            ZombieStateMachine.transform.rotation = newRotation;
        }
        else
        if (!ZombieStateMachine.useRootRotation && !ZombieStateMachine.targetReached) {
            Quaternion newRotation = Quaternion.LookRotation(ZombieStateMachine.navAgent.desiredVelocity);
            ZombieStateMachine.transform.rotation = Quaternion.Slerp(ZombieStateMachine.transform.rotation, newRotation, SlerpSpeed * Time.deltaTime);
        }
        else
        if (ZombieStateMachine.targetReached) {
            return (AIStateType.Alerted);
        }

        // Do we have a visual threat that is player.
        if (ZombieStateMachine.VisualThreat.type == AITargetType.Visual_Player) {
            if (ZombieStateMachine.targetPosition != ZombieStateMachine.VisualThreat.position) {
                if (Mathf.Clamp(ZombieStateMachine.VisualThreat.distance * RepathDistanceMultiplier, RepathVisualMinDuration, RepathVisualMaxDuration) < RepathTimer) {
                    ZombieStateMachine.navAgent.SetDestination(ZombieStateMachine.VisualThreat.position);
                    RepathTimer = 0.0f;
                }
            }
            ZombieStateMachine.SetTarget(ZombieStateMachine.VisualThreat);
            return (AIStateType.Pursuit);
        }

        if (ZombieStateMachine.targetType == AITargetType.Visual_Player) {
            return (AIStateType.Pursuit);
        }

        if (ZombieStateMachine.VisualThreat.type == AITargetType.Visual_Light) {
            if (ZombieStateMachine.targetType == AITargetType.Audio || ZombieStateMachine.targetType == AITargetType.Visual_Food) {
                ZombieStateMachine.SetTarget(ZombieStateMachine.VisualThreat);
                return (AIStateType.Alerted);
            }
            else
            if (ZombieStateMachine.targetType == AITargetType.Visual_Light) {
                int currentId = ZombieStateMachine.targetColliderId;
                if (currentId == ZombieStateMachine.VisualThreat.collider.GetInstanceID()) {
                    if (Mathf.Clamp(ZombieStateMachine.VisualThreat.distance * RepathDistanceMultiplier, RepathVisualMinDuration, RepathAudioMaxDuration) < RepathTimer) {
                        ZombieStateMachine.navAgent.SetDestination(ZombieStateMachine.VisualThreat.position);
                        RepathTimer = 0.0f;
                    }
                }
                ZombieStateMachine.navAgent.SetDestination(ZombieStateMachine.VisualThreat.position);
                return (AIStateType.Pursuit);
            } else {
                ZombieStateMachine.SetTarget(ZombieStateMachine.VisualThreat);
                return (AIStateType.Alerted);
            }
        }
        else
        if (ZombieStateMachine.AudioThreat.type == AITargetType.Audio) {
            if (ZombieStateMachine.targetType == AITargetType.Visual_Food) {
                ZombieStateMachine.SetTarget(ZombieStateMachine.AudioThreat);
                return (AIStateType.Alerted);
            }
            else
            if (ZombieStateMachine.targetType == AITargetType.Audio) {
                int currentId = ZombieStateMachine.targetColliderId;
                if (currentId == ZombieStateMachine.AudioThreat.collider.GetInstanceID()) {
                    if (ZombieStateMachine.targetPosition != ZombieStateMachine.AudioThreat.position) {
                        if (Mathf.Clamp(ZombieStateMachine.AudioThreat.distance * RepathDistanceMultiplier, RepathAudioMinDuration, RepathAudioMaxDuration) < RepathTimer) {
                            ZombieStateMachine.navAgent.SetDestination(ZombieStateMachine.AudioThreat.position);
                            RepathTimer = 0.0f;
                        }
                    }
                    ZombieStateMachine.navAgent.SetDestination(ZombieStateMachine.AudioThreat.position);
                    return (AIStateType.Pursuit);
                } else {
                    ZombieStateMachine.navAgent.SetDestination(ZombieStateMachine.AudioThreat.position);
                    return (AIStateType.Alerted);
                }
            }
        }
        return (AIStateType.Pursuit);
    }
}
