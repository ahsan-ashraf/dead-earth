using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AIStateType     { None, Idle, Alerted, Patrol, Attack, Feeding, Pursuit, Dead } // Crawl
public enum AITargetType    { None, WayPoint, Visual_Player, Visual_Light, Visual_Food, Audio }
public enum AITriggerEvent  { Enter, Stay, Exit }

public struct AITarget {
    private AITargetType    Type;
    private Collider        Collider;
    private Vector3         Position;
    private float           Distance;
    private float           Time;

    public AITargetType type        { get { return Type; } }
    public Collider     collider    { get { return Collider; } }
    public Vector3      position    { get { return Position; } }
    public float        distance    { get { return Distance; } set { Distance = value; } }
    public float        time        { get { return Time; } }

    public void Set(AITargetType t, Collider c, Vector3 p, float d) {
        Type        = t;
        Collider    = c;
        Position    = p;
        Distance    = d;
        Time        = UnityEngine.Time.deltaTime;
    }
    public void Clear() {
        Type        = AITargetType.None;
        Collider    = null;
        Position    = Vector3.zero;
        Distance    = Mathf.Infinity;
        Time        = 0.0f;
    }
}

public abstract class AIStateMachine : MonoBehaviour {

    public AITarget VisualThreat    = new AITarget();
    public AITarget AudioThreat     = new AITarget();

    protected   Dictionary<AIStateType, AIState>    States          = new Dictionary<AIStateType, AIState>();
    protected   AIState                             CurrentState    = null;
    protected   AITarget                            Target          = new AITarget();

    [SerializeField] protected  AIStateType     CurrentStateType    = AIStateType.Idle;
    [SerializeField] protected  SphereCollider  TargetTrigger       = null;
    [SerializeField] protected  SphereCollider  SensorTrigger       = null;

    [SerializeField] [Range(0.0f, 15f)] protected float     StoppingDistance = 1.0f;

    protected   Animator        Animator    = null;
    protected   NavMeshAgent    NavAgent    = null;
    protected   Collider        Collider    = null;
    //protected   Transform       Transform   = null;

    public Animator     animator    { get { return Animator; } }
    public NavMeshAgent navAgent    { get { return NavAgent; } }

    protected virtual void Awake() {
        Animator    = GetComponent<Animator>();
        NavAgent    = GetComponent<NavMeshAgent>();
        Collider    = GetComponent<Collider>();
    }
    protected virtual void Start() {
        // Fetch all states on this gameObject and add them into the States Dictionary.
        AIState[] aiStates = GetComponents<AIState>();
        foreach (AIState state in aiStates) {
            if (state != null && !States.ContainsKey(state.GetStateType())) {
                States.Add(state.GetStateType(), state);
                state.SetStateMachine(this);
            }
        }
        if (States.ContainsKey(CurrentStateType)) {
            CurrentState = States[CurrentStateType];
            CurrentState.OnEnterState();
        } else {
            CurrentState = null;
        }
    }
    protected virtual void Update() {
        if (CurrentState == null)   return;

        AIStateType newStateType = CurrentState.OnUpdate();
        if (newStateType != CurrentStateType) {
            AIState newState = null;
            if (States.TryGetValue(newStateType, out newState)) {
                TransitionToState(newState);
            } else if (States.TryGetValue(AIStateType.Idle, out newState)) {
                TransitionToState(newState);
            }
        }
        CurrentStateType = newStateType;
    }
    private void TransitionToState(AIState newState) {
        CurrentState.OnExitState();
        newState.OnEnterState();
        CurrentState = newState;
    }
    protected virtual void FixedUpdate() {
        VisualThreat.Clear();
        AudioThreat.Clear();

        if (Target.type != AITargetType.None) {
            Target.distance = Vector3.Distance(transform.position, Target.position);
        }
    }
    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d) {
        Target.Set(t, c, p, d);
        if (TargetTrigger != null) {
            TargetTrigger.radius                = StoppingDistance;
            TargetTrigger.transform.position    = Target.position;
            TargetTrigger.enabled               = true;
        }
    }
    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d, float s) {
        Target.Set(t, c, p, d);
        if (TargetTrigger != null) {
            TargetTrigger.radius                = s;
            TargetTrigger.transform.position    = Target.position;
            TargetTrigger.enabled               = true;
        }
    }
    public void SetTarget(AITarget t) {
        Target = t;
        if (TargetTrigger != null) {
            TargetTrigger.radius = StoppingDistance;
            TargetTrigger.transform.position = Target.position;
            TargetTrigger.enabled = true;
        }
    }
    public void ClearTarget() {
        Target.Clear();
        if (TargetTrigger != null) {
            TargetTrigger.enabled = false;
        }
    }
}
