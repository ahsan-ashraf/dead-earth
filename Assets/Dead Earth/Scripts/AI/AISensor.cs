using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISensor : MonoBehaviour {

    private AIStateMachine ParentStateMachine = null;

    public AIStateMachine parentStateMachine {
        set {
            ParentStateMachine = value;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (ParentStateMachine != null) {
            ParentStateMachine.OnTriggerEvent(AITriggerEventType.Enter, other);
        }
    }
    private void OnTriggerStay(Collider other) {
        if (ParentStateMachine != null) {
            ParentStateMachine.OnTriggerEvent(AITriggerEventType.Stay, other);
        }
    }
    private void OnTriggerExit(Collider other) {
        if (ParentStateMachine != null) {
            ParentStateMachine.OnTriggerEvent(AITriggerEventType.Exit, other);
        }
    }
}
