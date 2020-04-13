using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIZombieState : AIState {
    // Private.
    protected int PlayerLayerMask   = -1;
    protected int BodyPartLayerMask = -1;

    private void Awake() {
        //PlayerLayerMask = LayerMask.GetMask("Player", "AI Body Part", "Default")+1;
        PlayerLayerMask = LayerMask.GetMask("Player", "AI Body Part", "Default");
        BodyPartLayerMask = LayerMask.GetMask("AI Body Part");
    }

    public override void OnTriggerEvent(AITriggerEventType eventType, Collider other) {
        if (StateMachine != null)   return;
        
        if (eventType != AITriggerEventType.Exit) {
            AITargetType currentTargetType = StateMachine.VisualThreat.type;
            
            if (other.CompareTag("Player")) {
                float distance = Vector3.Distance(StateMachine.sensorPosition, other.transform.position);
                if (currentTargetType != AITargetType.Visual_Player || (currentTargetType == AITargetType.Visual_Player && distance < StateMachine.VisualThreat.distance)) {
                    RaycastHit hitInfo;
                    if (ColliderIsVisible(other, out hitInfo, PlayerLayerMask)) {
                        // Yep...its close and in our FOV so store as the current most dangerous threat.
                        StateMachine.VisualThreat.Set(AITargetType.Visual_Player, other, other.transform.position, distance);
                    }
                }
            }
        }
    }
    protected virtual bool ColliderIsVisible(Collider other, out RaycastHit hitInfo, int layerMask = -1) {
        hitInfo = new RaycastHit();
        if (StateMachine == null || StateMachine.GetType() != typeof(AIZombieStateMachine)){
            return (false);
        }
        AIZombieStateMachine zombieStateMachine = (AIZombieStateMachine)StateMachine;
        Vector3 head        = StateMachine.sensorPosition;
        Vector3 direction   = other.transform.position - head;
        float   angle       = Vector3.Angle(direction, transform.forward);

        if (angle > zombieStateMachine.fov * 0.5) {
            return false;
        } else {
            RaycastHit[] hits = Physics.RaycastAll(head, direction.normalized, zombieStateMachine.sensorRadius * zombieStateMachine.sight, layerMask);

            // Find clossest collider that is not the AIs own body part, if its not the target then the target is obstructed.
            float clossestColliderDistance = float.MaxValue;
            Collider clossestCollider = null;

            for (int i = 0; i < hits.Length; i++) {
                RaycastHit hit = hits[i];
                if (hit.distance < clossestColliderDistance) {
                    if (hit.transform.gameObject.layer == BodyPartLayerMask) {
                        if (StateMachine != GameSceneManager.Instance.GetStateMachine(hit.rigidbody.GetInstanceID())) {
                            clossestColliderDistance = hit.distance;
                            clossestCollider = hit.collider;
                            hitInfo = hit;
                        }
                    } else {
                        clossestColliderDistance = hit.distance;
                        clossestCollider = hit.collider;
                        hitInfo = hit;
                    }
                }
            }
            if (clossestCollider != null && clossestCollider.gameObject == other.gameObject) {
                return(true);
            } else {
                return(false);
            }
        }
    }
}
