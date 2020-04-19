using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Feeding1 : AIZombieState {

    [SerializeField] private float SlerpSpeed = 5.0f;
    
    private int EatingStateHash     = Animator.StringToHash("Feeding State");
    private int EatingLayerIndex    = -1;

    public override AIStateType GetStateType() {
        return (AIStateType.Feeding);
    }
    public override void OnEnterState() {
        base.OnEnterState();
        Debug.Log("Entering feeding state");
        if (ZombieStateMachine == null) return;

        if (EatingLayerIndex == -1) {
            EatingLayerIndex = ZombieStateMachine.animator.GetLayerIndex("Cinematic");
        }

        ZombieStateMachine.feeding      = true;
        ZombieStateMachine.seeking      = 0;
        ZombieStateMachine.speed        = 0.0f;
        ZombieStateMachine.attackType   = 0;

        ZombieStateMachine.NavAgentControl(true, false);
    }
    public override AIStateType OnUpdate() {
        if (ZombieStateMachine.satisfaction > 0.95f) {
            ZombieStateMachine.GetWayPointPosition(false);
            return (AIStateType.Alerted);
        }
        if (ZombieStateMachine.VisualThreat.type != AITargetType.None && ZombieStateMachine.VisualThreat.type != AITargetType.Visual_Food) {
            ZombieStateMachine.SetTarget(ZombieStateMachine.VisualThreat);
            return (AIStateType.Alerted);
        }
        if (ZombieStateMachine.AudioThreat.type == AITargetType.Audio) {
            ZombieStateMachine.SetTarget(ZombieStateMachine.AudioThreat);
            return (AIStateType.Alerted);
        }
        
        if (ZombieStateMachine.animator.GetCurrentAnimatorStateInfo(EatingLayerIndex).shortNameHash == EatingStateHash) {
            ZombieStateMachine.satisfaction = Mathf.Min(ZombieStateMachine.satisfaction + ((ZombieStateMachine.replenishRate * Time.deltaTime) / 100.0f), 1.0f);
        }

        if (!ZombieStateMachine.useRootRotation) {
            Vector3 targetPosition                  = ZombieStateMachine.targetPosition;
            targetPosition.y                        = ZombieStateMachine.transform.position.y;
            Quaternion newRotation                  = Quaternion.LookRotation((targetPosition - ZombieStateMachine.transform.position), Vector3.up);
            ZombieStateMachine.transform.rotation   = Quaternion.Slerp(ZombieStateMachine.transform.rotation, newRotation, SlerpSpeed * Time.deltaTime);
        }

        return (AIStateType.Feeding);
    }
    public override void OnExitState() {
        if (ZombieStateMachine != null) {
            base.OnExitState();
            ZombieStateMachine.feeding = false;
        }
    }
}
