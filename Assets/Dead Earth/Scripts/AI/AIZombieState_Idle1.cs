using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// AI State that implements the zombi's idle behaviour.
/// </summary>
public class AIZombieState_Idle1 : AIZombieState {
    
    [SerializeField] private Vector2 IdleTimeRange = new Vector2(10.0f, 60.0f);

    private float IdleTime  = 0.0f;
    private float Timer     = 0.0f;

    /// <summary>
    /// Called once by State Machine when transitioning into this state, to initialize this state.
    /// </summary>
    public override void OnEnterState() {
        base.OnEnterState();
        Debug.Log("Entering Idle State");
        if (ZombieStateMachine == null) return;

        IdleTime    = Random.Range(IdleTimeRange.x, IdleTimeRange.y);
        Timer       = 0.0f;
        ZombieStateMachine.NavAgentControl(true, false);
        ZombieStateMachine.speed        = 0.0f;
        ZombieStateMachine.seeking      = 0;
        ZombieStateMachine.feeding      = false;
        ZombieStateMachine.attackType   = 0;
        ZombieStateMachine.ClearTarget();
    }
    /// <summary>
    /// Called by State Machine each frame.
    /// Returns next state in which it should be transitioned.
    /// </summary>
    /// <returns></returns>
    public override AIStateType OnUpdate() {
        if (ZombieStateMachine == null) {
            return(AIStateType.Idle);
        } else {
            if (ZombieStateMachine.VisualThreat.type == AITargetType.Visual_Player) {
                ZombieStateMachine.SetTarget(ZombieStateMachine.VisualThreat);
                return(AIStateType.Pursuit);
            }
            if (ZombieStateMachine.VisualThreat.type == AITargetType.Visual_Light) {
                ZombieStateMachine.SetTarget(ZombieStateMachine.VisualThreat);
                return (AIStateType.Alerted);
            }
            if (ZombieStateMachine.AudioThreat.type == AITargetType.Audio) {
                ZombieStateMachine.SetTarget(ZombieStateMachine.AudioThreat);
                return (AIStateType.Alerted);
            }
            if (ZombieStateMachine.VisualThreat.type == AITargetType.Visual_Food) {
                ZombieStateMachine.SetTarget(ZombieStateMachine.VisualThreat);
                return (AIStateType.Pursuit);
            }
            Timer += Time.deltaTime;
            if (Timer > IdleTime) {
                return (AIStateType.Patrol);
            } else {
                return (AIStateType.Idle);
            }
        }
    }
    /// <summary>
    /// Returns the type of this state.
    /// </summary>
    /// <returns></returns>
    public override AIStateType GetStateType() {
        return (AIStateType.Idle);
    }
}
