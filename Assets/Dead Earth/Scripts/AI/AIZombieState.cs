using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIZombieState : AIState {
    // Private.
    protected int PlayerLayerMask   = -1;
    protected int BodyPartLayerMask = -1;
    protected int VisualLayerMask   = -1;
    protected AIZombieStateMachine ZombieStateMachine = null;

    private void Awake() {
        //PlayerLayerMask = LayerMask.GetMask("Player", "AI Body Part") + 1;
        PlayerLayerMask     = LayerMask.GetMask("Player", "AI Body Part", "Default");
        PlayerLayerMask     = LayerMask.GetMask("Player", "AI Body Part", "Default", "Visual Aggravator");
        BodyPartLayerMask   = LayerMask.NameToLayer("AI Body Part");
    }
    public override void SetStateMachine(AIStateMachine stateMachine) {
        if (stateMachine.GetType() == typeof(AIZombieStateMachine)) {
            base.SetStateMachine(stateMachine);
            ZombieStateMachine = (AIZombieStateMachine)stateMachine;
        }
    }
    public override void OnTriggerEvent(AITriggerEventType eventType, Collider other) {
        if (ZombieStateMachine == null) {
            return;
        }
        if (eventType != AITriggerEventType.Exit) {
            AITargetType currentTargetType = ZombieStateMachine.VisualThreat.type;
            
            if (other.CompareTag("Player")) {
                float distance = Vector3.Distance(ZombieStateMachine.sensorPosition, other.transform.position);
                if (currentTargetType != AITargetType.Visual_Player || (currentTargetType == AITargetType.Visual_Player && distance < ZombieStateMachine.VisualThreat.distance)) {
                    RaycastHit hitInfo;
                    if (ColliderIsVisible(other, out hitInfo, PlayerLayerMask)) {
                        // Yep...its close and in our FOV so store as the current most dangerous threat.
                        ZombieStateMachine.VisualThreat.Set(AITargetType.Visual_Player, other, other.transform.position, distance);
                    }
                }
            } 
            else
            if (other.CompareTag("Flash Light") && currentTargetType != AITargetType.Visual_Player) {
                BoxCollider flashLightTrigger = (BoxCollider)other;
                float distanceToThreat = Vector3.Distance(ZombieStateMachine.sensorPosition, flashLightTrigger.transform.position);
                float zSize = flashLightTrigger.size.z * flashLightTrigger.transform.lossyScale.z;
                float aggrFactor = distanceToThreat / zSize;
                if (aggrFactor < ZombieStateMachine.sight && aggrFactor <= ZombieStateMachine.intelligence) {
                    ZombieStateMachine.VisualThreat.Set(AITargetType.Visual_Light, other, other.transform.position, distanceToThreat);
                }
            } 
            else 
            if (other.CompareTag("AI Sound Emitter")) {
                SphereCollider soundTrigger = (SphereCollider)other;
                if (soundTrigger == null) {
                    return;
                }
                // Get Agent Sensor Position.
                Vector3 agentSensorPosition = ZombieStateMachine.sensorPosition;

                Vector3 soundPosition;
                float   soundRadius;
                AIState.ConvertSphereColliderToWorldSpace(soundTrigger, out soundPosition, out soundRadius);

                // How far inside the sound's radius are we.
                float distanceToThreat = (soundPosition - agentSensorPosition).magnitude;

                // Calculate a distance factor such that it is 1.0 when at sound radius 0 when at center.
                float distanceFactor = distanceToThreat / soundRadius;

                // Bias the factor based on hearing ability of Agent.
                distanceFactor += distanceFactor * (1.0f - ZombieStateMachine.hearing);

                // Too far away.
                if (distanceToThreat < ZombieStateMachine.AudioThreat.distance) {
                    ZombieStateMachine.AudioThreat.Set(AITargetType.Audio, other, soundPosition, distanceToThreat);
                }
            }
            else
            if (other.CompareTag("AI Food") && currentTargetType != AITargetType.Visual_Player && currentTargetType != AITargetType.Visual_Light 
                && ZombieStateMachine.AudioThreat.type == AITargetType.None && ZombieStateMachine.satisfaction <= 0.9f) {
                
                // How far away is the threat from us.
                float distanceToThreat = Vector3.Distance(other.transform.position, ZombieStateMachine.sensorPosition);

                // Is this smaller in distance from anything that we have stored.
                if (distanceToThreat < ZombieStateMachine.VisualThreat.distance) {

                    // If so then check that if it is in our FOV and it is within the range of this AI's sight.
                    RaycastHit hitInfo;
                    if (ColliderIsVisible(other, out hitInfo, VisualLayerMask)) {
                        // Yep this is our most appealing target so far.
                        ZombieStateMachine.VisualThreat.Set(AITargetType.Visual_Food, other, other.transform.position, distanceToThreat);
                    }
                }
            }
        }
    }
    protected virtual bool ColliderIsVisible(Collider other, out RaycastHit hitInfo, int layerMask = -1) {
        hitInfo = new RaycastHit();
        if (ZombieStateMachine == null) {
            return (false);
        }
        Vector3 head        = ZombieStateMachine.sensorPosition;
        Vector3 direction   = other.transform.position - head;
        float   angle       = Vector3.Angle(direction, transform.forward);

        if (angle > ZombieStateMachine.fov * 0.5) {
            return false;
        } else {
            RaycastHit[] hits = Physics.RaycastAll(head, direction.normalized, ZombieStateMachine.sensorRadius * ZombieStateMachine.sight, layerMask);

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
