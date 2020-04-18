using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIZombieState_Alerted1 : AIZombieState {

    [SerializeField] private float ThreatAngleThreshold     = 10.0f;
    [SerializeField] private float WayPointAngleThreshold   = 90.0f;

    [SerializeField] [Range(1.0f, 60.0f)] private float MaxDuration = 10.0f;

    private float Timer = 0.0f;

    public override void OnEnterState() {
        base.OnEnterState();
        Debug.Log("Entering Alerted State");

        if (ZombieStateMachine == null) return;

        ZombieStateMachine.NavAgentControl(true, false);
        ZombieStateMachine.speed        = 0.0f;
        ZombieStateMachine.seeking      = 0;
        ZombieStateMachine.feeding      = false;
        ZombieStateMachine.attackType   = 0;
        Timer                           = MaxDuration;
    }
    public override AIStateType OnUpdate() {

        Timer -= Time.deltaTime;
        if (Timer <= 0.0f) {
            return (AIStateType.Patrol);
        }

        if (ZombieStateMachine.VisualThreat.type == AITargetType.Visual_Player) {
            ZombieStateMachine.SetTarget(ZombieStateMachine.VisualThreat);
            return (AIStateType.Pursuit);
        }
        if (ZombieStateMachine.AudioThreat.type == AITargetType.Audio) {
            ZombieStateMachine.SetTarget(ZombieStateMachine.AudioThreat);
            Timer = MaxDuration;
        }
        if (ZombieStateMachine.VisualThreat.type == AITargetType.Visual_Light) {
            ZombieStateMachine.SetTarget(ZombieStateMachine.VisualThreat);
            Timer = MaxDuration;
        }
        if (ZombieStateMachine.AudioThreat.type == AITargetType.None && ZombieStateMachine.VisualThreat.type == AITargetType.Visual_Food) {
            ZombieStateMachine.SetTarget(ZombieStateMachine.VisualThreat);
            return (AIStateType.Pursuit);
        }
        
        float angle = 0.0f;
        if (ZombieStateMachine.targetType == AITargetType.Audio || ZombieStateMachine.targetType == AITargetType.Visual_Light) {

            angle = AIState.FindSignedAngle(ZombieStateMachine.transform.forward, (ZombieStateMachine.targetPosition - ZombieStateMachine.transform.position));
            if (ZombieStateMachine.targetType == AITargetType.Audio && Mathf.Abs(angle) < ThreatAngleThreshold) {
                return (AIStateType.Pursuit);
            }
            if (Random.value < ZombieStateMachine.intelligence) {
                ZombieStateMachine.seeking = -(int)Mathf.Sign(angle);
            }
            else {
                ZombieStateMachine.seeking = (int)Mathf.Sign(Random.Range(-1.0f, 1.0f));
            }
        } 
        else
        if (ZombieStateMachine.targetType == AITargetType.WayPoint) {
            angle = AIState.FindSignedAngle(ZombieStateMachine.transform.forward, (ZombieStateMachine.navAgent.steeringTarget - ZombieStateMachine.transform.position));
            if (Mathf.Abs(angle) < WayPointAngleThreshold) {
                return (AIStateType.Patrol);
            }
            ZombieStateMachine.seeking = -(int)Mathf.Sign(angle);
        }
        return (AIStateType.Alerted);
    }
    public override AIStateType GetStateType() {
        return (AIStateType.Alerted);
    }
}
