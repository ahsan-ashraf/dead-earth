using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootMotionConfigurator : AIStateMachineLink {
    [SerializeField] private int RootPosition = 0;
    [SerializeField] private int RootRotation = 0;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        if (stateMachine != null) {
            stateMachine.AddRootMotionRequest(RootPosition, RootRotation);
        }
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        if (stateMachine != null) {
            stateMachine.AddRootMotionRequest(-RootPosition, -RootRotation);
        }
    }
}
