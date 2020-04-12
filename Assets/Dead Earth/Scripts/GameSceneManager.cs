using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneManager : MonoBehaviour {

    public static GameSceneManager Instance = null;

    private Dictionary<int, AIStateMachine> StateMachines = new Dictionary<int, AIStateMachine>();
    
    /// <summary>
    /// MonoBehaviour Callback: used for referencing.
    /// </summary>
    private void Awake() {
        // Setting up Singleton.
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Instance = this;
        }
    }
    /// <summary>
    /// Registers the State Machine of colliders with instance id of Collider as a key.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="stateMachine"></param>
    public void RegisterStateMachine(int key, AIStateMachine stateMachine) {
        if (!StateMachines.ContainsKey(key)) {
            StateMachines.Add(key, stateMachine);
        }
    }
    /// <summary>
    /// Returns the AIStateMachine Reference related to the provided key/id.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public AIStateMachine GetStateMachine(int key) {
        AIStateMachine stateMachine;
        StateMachines.TryGetValue(key, out stateMachine);
        return(stateMachine);
    }
}
