using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIStateMachineLink : StateMachineBehaviour {

    private AIStateMachine StateMachine;
    public AIStateMachine stateMachine {
        get {
            return StateMachine;
        }
        set {
            StateMachine = value;
        }
    }
}
